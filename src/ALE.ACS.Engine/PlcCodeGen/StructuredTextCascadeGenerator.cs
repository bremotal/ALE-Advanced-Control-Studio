using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Cascade;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.PlcCodeGen;

/// <summary>
/// Gera um FUNCTION_BLOCK Cascade em Structured Text a partir de uma <see cref="CascadeSpec"/>.
/// Emite dois blocos PID (master + slave) inline, com handoff de SP e anti-windup
/// por back-calculation quando o slave satura.
/// </summary>
public sealed class StructuredTextCascadeGenerator : IPlcCodeGenerator
{
    public ControllerKind Supports => ControllerKind.Cascade;

    public PlcGenerationResult Generate(PlcGenerationRequest request)
    {
        if (request.Spec is not CascadeSpec spec)
            return PlcGenerationResult.Fail("Spec não é CascadeSpec.", request.Target);

        var w = new StructuredTextWriter();
        var blockName = $"{request.BlockPrefix}_CASCADE_{StructuredTextWriter.Id(spec.Name)}";

        if (request.EmitComments)
        {
            w.CommentBlock($"FUNCTION_BLOCK {blockName}");
            w.Comment($"Cascade master/slave — {spec.Name}");
            w.Comment($"Master: Kp={spec.MasterKp:G}, Ti={spec.MasterTi:G}s, Td={spec.MasterTd:G}s, N={spec.MasterN:G}");
            w.Comment($"Slave:  Kp={spec.SlaveKp:G}, Ti={spec.SlaveTi:G}s, Td={spec.SlaveTd:G}s, N={spec.SlaveN:G}");
            w.Comment($"SP_slave ∈ [{spec.MasterOutMin},{spec.MasterOutMax}], MV ∈ [{spec.SlaveOutMin},{spec.SlaveOutMax}]");
            w.Comment($"Scan={StructuredTextWriter.T(spec.ScanTime)}");
            w.Comment($"Gerado em {DateTime.UtcNow:u}");
            w.Blank();
        }

        w.Line($"FUNCTION_BLOCK {blockName}");

        // VAR_INPUT
        w.OpenBlock("VAR_INPUT");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Setpoint.Name, "LREAL",
            StructuredTextWriter.F(spec.Setpoint.Nominal ?? spec.Setpoint.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Controlled.Name, "LREAL",
            StructuredTextWriter.F(spec.Controlled.Nominal ?? spec.Controlled.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.SecondaryControlled.Name, "LREAL",
            StructuredTextWriter.F(spec.SecondaryControlled.Nominal ?? spec.SecondaryControlled.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Auto", "BOOL", "FALSE"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ManualValue", "LREAL",
            StructuredTextWriter.F(spec.SlaveOutMin)));
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR_OUTPUT
        w.OpenBlock("VAR_OUTPUT");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Manipulated.Name, "LREAL",
            StructuredTextWriter.F(spec.Manipulated.Nominal ?? spec.Manipulated.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "SP_Slave", "LREAL",
            StructuredTextWriter.F(spec.MasterOutMin)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Status", "INT", "0"));
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR — parâmetros + estado interno (master + slave)
        w.OpenBlock("VAR");
        if (request.EmitTuningAsConstants)
        {
            w.Comment("Master (slow loop)");
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "MasterKp", "LREAL", StructuredTextWriter.F(spec.MasterKp)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "MasterTi", "LREAL", StructuredTextWriter.F(spec.MasterTi)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "MasterTd", "LREAL", StructuredTextWriter.F(spec.MasterTd)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "MasterN", "LREAL", StructuredTextWriter.F(spec.MasterN)));
            w.Blank();
            w.Comment("Slave (fast loop)");
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "SlaveKp", "LREAL", StructuredTextWriter.F(spec.SlaveKp)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "SlaveTi", "LREAL", StructuredTextWriter.F(spec.SlaveTi)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "SlaveTd", "LREAL", StructuredTextWriter.F(spec.SlaveTd)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "SlaveN", "LREAL", StructuredTextWriter.F(spec.SlaveN)));
            w.Blank();
        }
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "MasterOutMin", "LREAL", StructuredTextWriter.F(spec.MasterOutMin)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "MasterOutMax", "LREAL", StructuredTextWriter.F(spec.MasterOutMax)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "SlaveOutMin", "LREAL", StructuredTextWriter.F(spec.SlaveOutMin)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "SlaveOutMax", "LREAL", StructuredTextWriter.F(spec.SlaveOutMax)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ScanTime", "TIME", StructuredTextWriter.T(spec.ScanTime)));
        w.Blank();
        w.Comment("Estado interno master");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "M_Integral", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "M_PvPrev", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "M_DerivState", "LREAL", "0.0"));
        w.Blank();
        w.Comment("Estado interno slave");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "S_Integral", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "S_PvPrev", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "S_DerivState", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Initialized", "BOOL", "FALSE"));
        w.CloseBlock("END_VAR");
        w.Blank();

        var spId = StructuredTextWriter.Id(spec.Setpoint.Name);
        var pv1Id = StructuredTextWriter.Id(spec.Controlled.Name);
        var pv2Id = StructuredTextWriter.Id(spec.SecondaryControlled.Name);
        var mvId = StructuredTextWriter.Id(spec.Manipulated.Name);

        // Método Run
        w.Line("METHOD PUBLIC Run : LREAL");
        w.OpenBlock("");
        w.Indent--;

        w.Comment("===== MASTER: SP_primary vs PV_primary → SP_slave =====");
        w.Line("e1 := SP_primary - PV_primary;");
        w.Line("p1 := MasterKp * e1;");
        w.Line("IF MasterTi > 0.0 THEN");
        w.Indent++; w.Line("M_Integral := M_Integral + MasterKp * TIME_TO_LREAL(ScanTime) / 1000.0 / MasterTi * e1;"); w.Indent--;
        w.Line("END_IF;");
        w.Line("IF MasterTd > 0.0 THEN");
        w.Indent++;
        w.Line("IF NOT Initialized THEN");
        w.Indent++; w.Line("M_PvPrev := PV_primary;"); w.Indent--;
        w.Line("END_IF;");
        w.Line("alpha1 := MasterTd / MasterN;");
        w.Line("coeff1 := MasterTd / (alpha1 + TIME_TO_LREAL(ScanTime) / 1000.0);");
        w.Line("M_DerivState := (M_DerivState + coeff1 * (PV_primary - M_PvPrev)) * (alpha1 / (alpha1 + TIME_TO_LREAL(ScanTime) / 1000.0));");
        w.Line("d1 := -M_DerivState;");
        w.Line("M_PvPrev := PV_primary;");
        w.Indent--;
        w.Line("ELSE");
        w.Indent++; w.Line("d1 := 0.0;"); w.Indent--;
        w.Line("END_IF;");
        w.Blank();
        w.Line("u1_raw := p1 + M_Integral + d1;");
        w.Line("SP_Slave := u1_raw;");
        w.Line("IF SP_Slave < MasterOutMin THEN SP_Slave := MasterOutMin; END_IF;");
        w.Line("IF SP_Slave > MasterOutMax THEN SP_Slave := MasterOutMax; END_IF;");
        w.Line("IF MasterTi > 0.0 AND SP_Slave <> u1_raw THEN");
        w.Indent++; w.Line("M_Integral := M_Integral - (u1_raw - SP_Slave);"); w.Indent--;
        w.Line("END_IF;");
        w.Blank();

        w.Comment("===== SLAVE: SP_slave vs PV_secondary → MV =====");
        w.Line("e2 := SP_Slave - PV_secondary;");
        w.Line("p2 := SlaveKp * e2;");
        w.Line("IF SlaveTi > 0.0 THEN");
        w.Indent++; w.Line("S_Integral := S_Integral + SlaveKp * TIME_TO_LREAL(ScanTime) / 1000.0 / SlaveTi * e2;"); w.Indent--;
        w.Line("END_IF;");
        w.Line("IF SlaveTd > 0.0 THEN");
        w.Indent++;
        w.Line("IF NOT Initialized THEN");
        w.Indent++; w.Line("S_PvPrev := PV_secondary;"); w.Line("Initialized := TRUE;"); w.Indent--;
        w.Line("END_IF;");
        w.Line("alpha2 := SlaveTd / SlaveN;");
        w.Line("coeff2 := SlaveTd / (alpha2 + TIME_TO_LREAL(ScanTime) / 1000.0);");
        w.Line("S_DerivState := (S_DerivState + coeff2 * (PV_secondary - S_PvPrev)) * (alpha2 / (alpha2 + TIME_TO_LREAL(ScanTime) / 1000.0));");
        w.Line("d2 := -S_DerivState;");
        w.Line("S_PvPrev := PV_secondary;");
        w.Indent--;
        w.Line("ELSE");
        w.Indent++; w.Line("d2 := 0.0;"); w.Indent--;
        w.Line("END_IF;");
        w.Blank();
        w.Line("u2_raw := p2 + S_Integral + d2;");
        w.Line("MV := u2_raw;");
        w.Line("IF MV < SlaveOutMin THEN MV := SlaveOutMin; END_IF;");
        w.Line("IF MV > SlaveOutMax THEN MV := SlaveOutMax; END_IF;");
        w.Line("IF SlaveTi > 0.0 AND MV <> u2_raw THEN");
        w.Indent++; w.Line("S_Integral := S_Integral - (u2_raw - MV);"); w.Indent--;
        w.Line("END_IF;");
        w.Blank();

        w.Comment("Auto/Manual (bumpless)");
        w.Line("IF NOT Auto THEN");
        w.Indent++;
        w.Line("MV := ManualValue;");
        w.Line("S_Integral := ManualValue - p2;");
        w.Indent--;
        w.Line("END_IF;");
        w.Blank();
        w.Line("Run := MV;");
        w.CloseBlock("END_METHOD");
        w.Blank();

        w.Line("METHOD PUBLIC Reset");
        w.OpenBlock("");
        w.Indent--;
        w.Line("M_Integral := 0.0; M_PvPrev := 0.0; M_DerivState := 0.0;");
        w.Line("S_Integral := 0.0; S_PvPrev := 0.0; S_DerivState := 0.0;");
        w.Line("Initialized := FALSE;");
        w.CloseBlock("END_METHOD");

        w.CloseBlock(PlcPlatformQuirks.EndBlockKeyword(request.Target, asProgram: false));

        // Renomeia SP_primary / PV_primary / PV_secondary / MV para os ids reais
        var st = w.Build()
            .Replace("SP_primary", spId)
            .Replace("PV_primary", pv1Id)
            .Replace("PV_secondary", pv2Id);

        return PlcGenerationResult.Ok(blockName, st, request.Target);
    }
}
