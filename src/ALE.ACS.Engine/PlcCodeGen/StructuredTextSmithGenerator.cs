using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.SmithPredictor;

namespace ALE.ACS.Engine.PlcCodeGen;

/// <summary>
/// Gera um FUNCTION_BLOCK Smith Predictor em Structured Text a partir de uma <see cref="SmithSpec"/>.
/// Emite o PID primário + o preditor FOPDT (parte sem atraso + buffer circular da MV).
/// </summary>
public sealed class StructuredTextSmithGenerator : IPlcCodeGenerator
{
    public ControllerKind Supports => ControllerKind.SmithPredictor;

    public PlcGenerationResult Generate(PlcGenerationRequest request)
    {
        if (request.Spec is not SmithSpec spec)
            return PlcGenerationResult.Fail("Spec não é SmithSpec.", request.Target);

        var w = new StructuredTextWriter();
        var blockName = $"{request.BlockPrefix}_SMITH_{StructuredTextWriter.Id(spec.Name)}";

        var model = spec.Model;
        var (a, b, d) = model.Discretize();
        var dInt = Math.Max(1, d);

        if (request.EmitComments)
        {
            w.CommentBlock($"FUNCTION_BLOCK {blockName}");
            w.Comment($"Smith Predictor — {spec.Name}");
            w.Comment($"PID: Kp={spec.Kp:G}, Ti={spec.Ti:G}s, Td={spec.Td:G}s, N={spec.N:G}");
            w.Comment($"Modelo FOPDT: K={model.K:G}, τ={model.Tau:G}s, θ={model.Theta:G}s, Ts={model.Ts:G}s");
            w.Comment($"Discretizado: a={a:G}, b={b:G}, dead-time samples={dInt}");
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
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Auto", "BOOL", "FALSE"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ManualValue", "LREAL",
            StructuredTextWriter.F(spec.OutMin)));
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR_OUTPUT
        w.OpenBlock("VAR_OUTPUT");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Manipulated.Name, "LREAL",
            StructuredTextWriter.F(spec.Manipulated.Nominal ?? spec.Manipulated.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "PredictedPv", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "PvApparent", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Status", "INT", "0"));
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR
        w.OpenBlock("VAR");
        if (request.EmitTuningAsConstants)
        {
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Kp", "LREAL", StructuredTextWriter.F(spec.Kp)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Ti", "LREAL", StructuredTextWriter.F(spec.Ti)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Td", "LREAL", StructuredTextWriter.F(spec.Td)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "N", "LREAL", StructuredTextWriter.F(spec.N)));
            w.Blank();
            w.Comment("Modelo FOPDT discretizado");
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ModelK", "LREAL", StructuredTextWriter.F(model.K)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ModelA", "LREAL", StructuredTextWriter.F(a)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ModelB", "LREAL", StructuredTextWriter.F(b)));
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, "DeadTimeSamples", "INT", dInt.ToString()));
            w.Blank();
        }
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "OutMin", "LREAL", StructuredTextWriter.F(spec.OutMin)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "OutMax", "LREAL", StructuredTextWriter.F(spec.OutMax)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ScanTime", "TIME", StructuredTextWriter.T(spec.ScanTime)));
        w.Blank();
        w.Comment("Estado interno do PID");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Integral", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "PvPrev", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "DerivState", "LREAL", "0.0"));
        w.Blank();
        w.Comment("Estado do preditor");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "YPred", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "YWithDelay", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "MvIndex", "INT", "0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Initialized", "BOOL", "FALSE"));
        w.Blank();
        w.Comment("Buffer circular de MV atrasada");
        w.Line($"MvHistory : {PlcPlatformQuirks.ArraySyntax(request.Target, 0, dInt - 1, "LREAL")};");
        w.CloseBlock("END_VAR");
        w.Blank();

        var spId = StructuredTextWriter.Id(spec.Setpoint.Name);
        var pvId = StructuredTextWriter.Id(spec.Controlled.Name);
        var mvId = StructuredTextWriter.Id(spec.Manipulated.Name);

        // Run
        w.Line("METHOD PUBLIC Run : LREAL");
        w.OpenBlock("");
        w.Indent--;

        w.Comment("Recupera MV atrasada do buffer circular");
        w.Line("uDelayed := MvHistory[MvIndex];");
        w.Blank();

        w.Comment("Predição sem atraso: y_pred(k) = a*y_pred(k-1) + b*u(k)");
        w.Line($"YPred := ModelA * YPred + ModelB * {mvId};");
        w.Line("IF NOT Initialized THEN");
        w.Indent++;
        w.Line($"YPred := {pvId};");
        w.Line("YWithDelay := {pvId};");
        w.Line("Initialized := TRUE;");
        w.Indent--;
        w.Line("END_IF;");
        w.Blank();

        w.Comment("Predição com atraso: y_withDelay(k) = a*y(k-1) + b*u(k-d)");
        w.Line("YWithDelay := ModelA * YWithDelay + ModelB * uDelayed;");
        w.Blank();

        w.Comment("PV aparente = y_pred + (PV_real - y_withDelay)");
        w.Line($"PvApparent := YPred + ({pvId} - YWithDelay);");
        w.Line("PredictedPv := YPred;");
        w.Blank();

        w.Comment("PID sobre (SP - PvApparent)");
        w.Line($"e := {spId} - PvApparent;");
        w.Line("p := Kp * e;");
        w.Line("IF Ti > 0.0 THEN");
        w.Indent++; w.Line("Integral := Integral + Kp * TIME_TO_LREAL(ScanTime) / 1000.0 / Ti * e;"); w.Indent--;
        w.Line("END_IF;");
        w.Line("IF Td > 0.0 THEN");
        w.Indent++;
        w.Line("IF NOT Initialized THEN");
        w.Indent++; w.Line($"PvPrev := PvApparent;"); w.Indent--;
        w.Line("END_IF;");
        w.Line("alpha := Td / N;");
        w.Line("coeff := Td / (alpha + TIME_TO_LREAL(ScanTime) / 1000.0);");
        w.Line("DerivState := (DerivState + coeff * (PvApparent - PvPrev)) * (alpha / (alpha + TIME_TO_LREAL(ScanTime) / 1000.0));");
        w.Line("d_val := -DerivState;");
        w.Line("PvPrev := PvApparent;");
        w.Indent--;
        w.Line("ELSE");
        w.Indent++; w.Line("d_val := 0.0;"); w.Indent--;
        w.Line("END_IF;");
        w.Blank();

        w.Comment("Saída + saturação + back-calculation");
        w.Line("u_raw := p + Integral + d_val;");
        w.Line($"u := u_raw;");
        w.Line("IF u < OutMin THEN u := OutMin; END_IF;");
        w.Line("IF u > OutMax THEN u := OutMax; END_IF;");
        w.Line("IF Ti > 0.0 AND u <> u_raw THEN");
        w.Indent++; w.Line("Integral := Integral - (u_raw - u);"); w.Indent--;
        w.Line("END_IF;");
        w.Blank();

        w.Comment("Atualiza buffer de MV atrasada");
        w.Line("MvHistory[MvIndex] := u;");
        w.Line("MvIndex := (MvIndex + 1) MOD DeadTimeSamples;");
        w.Blank();

        w.Comment("Auto/Manual");
        w.Line("IF Auto THEN");
        w.Indent++; w.Line($"{mvId} := u;"); w.Indent--;
        w.Line("ELSE");
        w.Indent++;
        w.Line($"{mvId} := ManualValue;");
        w.Line("Integral := ManualValue - p;");
        w.Indent--;
        w.Line("END_IF;");
        w.Blank();
        w.Line("Run := {mvId};");
        w.CloseBlock("END_METHOD");
        w.Blank();

        w.Line("METHOD PUBLIC Reset");
        w.OpenBlock("");
        w.Indent--;
        w.Line("Integral := 0.0; PvPrev := 0.0; DerivState := 0.0;");
        w.Line("YPred := 0.0; YWithDelay := 0.0; MvIndex := 0; Initialized := FALSE;");
        w.CloseBlock("END_METHOD");

        w.CloseBlock(PlcPlatformQuirks.EndBlockKeyword(request.Target, asProgram: false));

        // Patch: a referência a `mvId` na linha `YPred := ModelA * YPred + ModelB * {mvId};`
        // é o MV atual (entrada). Em runtime, o caller deve escrever o MV de execução
        // anterior antes de chamar Run. A linha `Run := {mvId}` apenas devolve o MV calculado.
        var st = w.Build();

        return PlcGenerationResult.Ok(blockName, st, request.Target);
    }
}
