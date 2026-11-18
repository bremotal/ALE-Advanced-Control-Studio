using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Cascade;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.PlcCodeGen;

/// <summary>
/// Gera um FUNCTION_BLOCK PID em Structured Text a partir de uma <see cref="PidSpec"/>.
/// Implementa a forma ISA com anti-windup por back-calculation e derivação sobre PV.
/// </summary>
public sealed class StructuredTextPidGenerator : IPlcCodeGenerator
{
    public ControllerKind Supports => ControllerKind.Pid;

    public PlcGenerationResult Generate(PlcGenerationRequest request)
    {
        if (request.Spec is not PidSpec spec)
            return PlcGenerationResult.Fail("Spec não é PidSpec.", request.Target);

        var w = new StructuredTextWriter();
        var blockName = $"{request.BlockPrefix}_PID_{StructuredTextWriter.Id(spec.Name)}";

        if (request.EmitComments)
        {
            w.CommentBlock($"FUNCTION_BLOCK {blockName}");
            w.Comment($"PID ISA — {spec.Name}");
            w.Comment($"Kp={spec.Kp:G}, Ti={spec.Ti:G}s, Td={spec.Td:G}s, N={spec.N:G}");
            w.Comment($"DirectAction={spec.DirectAction}, Out=[{spec.OutMin},{spec.OutMax}]");
            w.Comment($"Scan={StructuredTextWriter.T(spec.ScanTime)}");
            w.Comment($"Gerado em {DateTime.UtcNow:u}");
            w.Blank();
        }

        w.Line($"FUNCTION_BLOCK {blockName}");

        // VAR_INPUT
        w.OpenBlock("VAR_INPUT");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Controlled.Name, "LREAL",
            StructuredTextWriter.F(spec.Controlled.Nominal ?? spec.Controlled.Min)));
        if (request.EmitComments)
            w.Comment($"PV ({spec.Controlled.Unit.Symbol}) [{spec.Controlled.Min}..{spec.Controlled.Max}]");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Setpoint.Name, "LREAL",
            StructuredTextWriter.F(spec.Setpoint.Nominal ?? spec.Setpoint.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Auto", "BOOL", "FALSE"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ManualValue", "LREAL",
            StructuredTextWriter.F(spec.OutMin)));
        foreach (var d in spec.Disturbances)
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, d.Name, "LREAL",
                StructuredTextWriter.F(d.Nominal ?? d.Min)));
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR_OUTPUT
        w.OpenBlock("VAR_OUTPUT");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Manipulated.Name, "LREAL",
            StructuredTextWriter.F(spec.Manipulated.Nominal ?? spec.Manipulated.Min)));
        if (request.EmitComments)
            w.Comment($"MV ({spec.Manipulated.Unit.Symbol}) [{spec.Manipulated.Min}..{spec.Manipulated.Max}]");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Status", "INT", "0"));
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR — parâmetros + estado interno
        w.OpenBlock("VAR");
        if (request.EmitTuningAsConstants)
        {
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Kp", "LREAL", StructuredTextWriter.F(spec.Kp)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Ti", "LREAL", StructuredTextWriter.F(spec.Ti)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Td", "LREAL", StructuredTextWriter.F(spec.Td)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "N", "LREAL", StructuredTextWriter.F(spec.N)));
        }
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "OutMin", "LREAL", StructuredTextWriter.F(spec.OutMin)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "OutMax", "LREAL", StructuredTextWriter.F(spec.OutMax)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ScanTime", "TIME", StructuredTextWriter.T(spec.ScanTime)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "DirectAction", "BOOL", StructuredTextWriter.B(spec.DirectAction)));
        w.Blank();
        w.Comment("Estado interno");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Integral", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "PvPrev", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "DerivState", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Initialized", "BOOL", "FALSE"));
        w.CloseBlock("END_VAR");
        w.Blank();

        // Método principal
        w.Line($"METHOD PUBLIC Run : LREAL");
        w.OpenBlock("");
        w.Indent--;  // OpenBlock incrementou

        w.Comment("Erro (com convenção de ação)");
        var spId = StructuredTextWriter.Id(spec.Setpoint.Name);
        var pvId = StructuredTextWriter.Id(spec.Controlled.Name);
        var mvId = StructuredTextWriter.Id(spec.Manipulated.Name);
        w.Line($"IF DirectAction THEN");
        w.Indent++; w.Line($"e := {pvId} - {spId};"); w.Indent--;
        w.Line($"ELSE");
        w.Indent++; w.Line($"e := {spId} - {pvId};"); w.Indent--;
        w.Line($"END_IF;");
        w.Blank();

        w.Comment("Termo proporcional");
        w.Line($"p := Kp * e;");
        w.Blank();

        w.Comment("Termo integral (com anti-windup via back-calculation)");
        w.Line($"IF Ti > 0.0 THEN");
        w.Indent++;
        w.Line($"Integral := Integral + Kp * TIME_TO_LREAL(ScanTime) / 1000.0 / Ti * e;");
        w.Indent--;
        w.Line($"END_IF;");
        w.Line($"i_val := Integral;");
        w.Blank();

        w.Comment("Termo derivativo (sobre PV, com filtro de 1a ordem)");
        w.Line($"IF Td > 0.0 THEN");
        w.Indent++;
        w.Line($"IF NOT Initialized THEN");
        w.Indent++; w.Line($"PvPrev := pv;"); w.Line($"Initialized := TRUE;"); w.Indent--;
        w.Line($"END_IF;");
        w.Line($"alpha := Td / N;");
        w.Line($"coeff := Td / (alpha + TIME_TO_LREAL(ScanTime) / 1000.0);");
        w.Line($"DerivState := (DerivState + coeff * ({pvId} - PvPrev)) * (alpha / (alpha + TIME_TO_LREAL(ScanTime) / 1000.0));");
        w.Line($"d_val := -DerivState;");
        w.Line($"PvPrev := pv;");
        w.Indent--;
        w.Line($"ELSE");
        w.Indent++;
        w.Line($"d_val := 0.0;");
        w.Indent--;
        w.Line($"END_IF;");
        w.Blank();

        w.Comment("Saída bruta + saturação + back-calculation");
        w.Line($"u_raw := p + i_val + d_val;");
        w.Line($"u := u_raw;");
        w.Line($"IF u < OutMin THEN u := OutMin; END_IF;");
        w.Line($"IF u > OutMax THEN u := OutMax; END_IF;");
        w.Line($"IF Ti > 0.0 AND u <> u_raw THEN");
        w.Indent++;
        w.Line($"Integral := Integral - (u_raw - u);");
        w.Indent--;
        w.Line($"END_IF;");
        w.Blank();

        w.Comment("Auto/Manual");
        w.Line($"IF Auto THEN");
        w.Indent++; w.Line($"{mvId} := u;"); w.Indent--;
        w.Line($"ELSE");
        w.Indent++;
        w.Line($"{mvId} := ManualValue;");
        w.Line($"Integral := ManualValue - p;  // bumpless transfer");
        w.Indent--;
        w.Line($"END_IF;");
        w.Blank();
        w.Line($"Run := mvId;");
        w.CloseBlock("END_METHOD");
        w.Blank();

        // Método Reset
        w.Line($"METHOD PUBLIC Reset");
        w.OpenBlock("");
        w.Indent--;
        w.Line($"Integral := 0.0;");
        w.Line($"DerivState := 0.0;");
        w.Line($"PvPrev := 0.0;");
        w.Line($"Initialized := FALSE;");
        w.CloseBlock("END_METHOD");

        w.CloseBlock(PlcPlatformQuirks.EndBlockKeyword(request.Target, asProgram: false));

        return PlcGenerationResult.Ok(blockName, w.Build(), request.Target);
    }
}
