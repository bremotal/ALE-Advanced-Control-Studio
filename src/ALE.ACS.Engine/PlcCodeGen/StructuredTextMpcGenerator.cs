using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Mpc;
using ALE.ACS.Engine.Optimization;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.PlcCodeGen;

/// <summary>
/// Gera Structured Text para MPC usando a matriz de ganho K pré-computada (sem QP no PLC).
/// Estrutura: FB com matriz K como constante, observador Luenberger simples, e
/// Δu = -K * (x̂ - x_ref) por scan.
/// </summary>
public sealed class StructuredTextMpcGenerator : IPlcCodeGenerator
{
    public ControllerKind Supports => ControllerKind.Mpc;

    public PlcGenerationResult Generate(PlcGenerationRequest request)
    {
        if (request.Spec is not MpcSpec spec)
            return PlcGenerationResult.Fail("Spec não é MpcSpec.", request.Target);

        var design = (MpcDesign)spec.Design();
        if (!design.IsValid || design.K is null)
            return PlcGenerationResult.Fail($"MPC design inválido: {design.Summary}", request.Target);

        var warnings = new List<string>();
        if (spec.Model.Inputs != 1 || spec.Model.Outputs != 1)
            warnings.Add("Geração ST de MPC suporta apenas SISO neste pacote; MIMO requer extensão.");

        var K = design.K;
        var w = new StructuredTextWriter();
        var blockName = $"{request.BlockPrefix}_MPC_{StructuredTextWriter.Id(spec.Name)}";

        if (request.EmitComments)
        {
            w.CommentBlock($"FUNCTION_BLOCK {blockName}");
            w.Comment($"MPC — {spec.Name}");
            w.Comment($"Hp={spec.PredictionHorizon}, Hu={spec.ControlHorizon}, n={spec.Model.States}");
            w.Comment($"Q={spec.OutputWeight}, R={spec.InputWeight}");
            w.Comment($"MV=[{spec.InputMin},{spec.InputMax}]");
            w.Comment($"Scan={StructuredTextWriter.T(spec.ScanTime)}");
            w.Comment($"Gain matrix K pré-computada offline ({K.Rows}×{K.Cols}).");
            w.Comment($"Gerado em {DateTime.UtcNow:u}");
            w.Blank();
        }

        w.Line($"FUNCTION_BLOCK {blockName}");

        // VAR_INPUT
        w.OpenBlock("VAR_INPUT");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Controlled.Name, "LREAL",
            StructuredTextWriter.F(spec.Controlled.Nominal ?? spec.Controlled.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Setpoint.Name, "LREAL",
            StructuredTextWriter.F(spec.Setpoint.Nominal ?? spec.Setpoint.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Auto", "BOOL", "FALSE"));
        foreach (var d in spec.Disturbances)
            w.Line(PlcPlatformQuirks.VarDecl(request.Target, d.Name, "LREAL",
                StructuredTextWriter.F(d.Nominal ?? d.Min)));
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR_OUTPUT
        w.OpenBlock("VAR_OUTPUT");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Manipulated.Name, "LREAL",
            StructuredTextWriter.F(spec.Manipulated.Nominal ?? spec.Manipulated.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Status", "INT", "0"));
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR — estado, parâmetros, ganho K como constante
        w.OpenBlock("VAR CONSTANT");
        w.Line($"Hp : INT := {spec.PredictionHorizon};");
        w.Line($"Hu : INT := {spec.ControlHorizon};");
        w.Line($"n : INT := {spec.Model.States};");
        w.Line($"q : LREAL := {StructuredTextWriter.F(spec.OutputWeight)};");
        w.Line($"r : LREAL := {StructuredTextWriter.F(spec.InputWeight)};");
        w.Line($"MVMin : LREAL := {StructuredTextWriter.F(spec.InputMin)};");
        w.Line($"MVMax : LREAL := {StructuredTextWriter.F(spec.InputMax)};");
        w.CloseBlock("END_VAR");
        w.Blank();

        w.OpenBlock("VAR");
        // Matrizes A, B, C como ARRAYs constantes
        EmitMatrixConstant(w, request.Target, "A", spec.Model.A);
        EmitMatrixConstant(w, request.Target, "B", spec.Model.B);
        EmitMatrixConstant(w, request.Target, "C", spec.Model.C);
        EmitMatrixConstant(w, request.Target, "K", K);
        w.Blank();
        w.Comment("Estado estimado x̂ (vetor de n estados)");
        w.Line($"xhat : {PlcPlatformQuirks.ArraySyntax(request.Target, 0, spec.Model.States - 1, "LREAL")};");
        w.Comment("MV anterior (para cálculo de Δu)");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "u_prev", "LREAL", "0.0"));
        w.CloseBlock("END_VAR");
        w.Blank();

        // Método Run
        w.Line($"METHOD PUBLIC Run : LREAL");
        w.OpenBlock("");
        w.Indent--;

        w.Comment("1. Atualiza observador (Luenberger simplificado: x̂ ≈ modelo puro)");
        w.Comment("   x̂(k+1) = A x̂(k) + B u(k-1)");
        w.Line($"FOR i := 0 TO n - 1 DO");
        w.Indent++;
        w.Line($"x_new := 0.0;");
        w.Line($"FOR j := 0 TO n - 1 DO");
        w.Indent++;
        w.Line($"x_new := x_new + A[i, j] * xhat[j];");
        w.Indent--;
        w.Line($"END_FOR;");
        w.Line($"x_new := x_new + B[i, 0] * u_prev;");
        w.Indent--;
        w.Line($"END_FOR;");
        w.Blank();

        w.Comment("2. Calcula Δu = -K * (x̂ - x_ref), onde x_ref é o estado que produz SP");
        var spId = StructuredTextWriter.Id(spec.Setpoint.Name);
        var pvId = StructuredTextWriter.Id(spec.Controlled.Name);
        var mvId = StructuredTextWriter.Id(spec.Manipulated.Name);
        w.Line($"du := 0.0;");
        w.Line($"FOR i := 0 TO Hu - 1 DO");
        w.Indent++;
        w.Line($"FOR j := 0 TO n - 1 DO");
        w.Indent++;
        w.Line($"du := du - K[i, j] * (xhat[j]);");
        w.Indent--;
        w.Line($"END_FOR;");
        w.Comment("Termo de SP: -K[i, n] * (-SP) = +K[i, n] * SP");
        w.Line($"du := du + K[i, n] * {spId};");
        w.Indent--;
        w.Line($"END_FOR;");
        w.Blank();

        w.Comment("3. Aplica apenas o primeiro Δu (receding horizon)");
        w.Line($"u := u_prev + du;");
        w.Line($"IF u < MVMin THEN u := MVMin; END_IF;");
        w.Line($"IF u > MVMax THEN u := MVMax; END_IF;");
        w.Blank();

        w.Comment("4. Atualiza estado + saída");
        w.Line($"IF Auto THEN");
        w.Indent++;
        w.Line($"u_prev := u;");
        w.Line($"FOR i := 0 TO n - 1 DO xhat[i] := x_new; END_FOR;");
        w.Indent--;
        w.Line($"ELSE");
        w.Indent++;
        w.Line($"u_prev := 0.0;  // bumpless quando sai de Auto");
        w.Indent--;
        w.Line($"END_IF;");
        w.Blank();
        w.Line($"{mvId} := u;");
        w.Line($"Run := mvId;");
        w.CloseBlock("END_METHOD");

        w.CloseBlock(PlcPlatformQuirks.EndBlockKeyword(request.Target, asProgram: false));

        return PlcGenerationResult.Ok(blockName, w.Build(), request.Target, warnings);
    }

    private static void EmitMatrixConstant(StructuredTextWriter w, PlcTarget target, string name, Matrix M)
    {
        w.Comment($"Matriz {name} ({M.Rows}×{M.Cols}) — modelo/ganho pré-computado offline");
        w.Line($"{name} : {PlcPlatformQuirks.ArraySyntax(target, 0, M.Rows - 1, PlcPlatformQuirks.ArraySyntax(target, 0, M.Cols - 1, "LREAL"))} := [");
        w.Indent++;
        for (int i = 0; i < M.Rows; i++)
        {
            var row = string.Join(", ", Enumerable.Range(0, M.Cols).Select(j => StructuredTextWriter.F(M[i, j])));
            w.Line(i < M.Rows - 1 ? $"[{row}]," : $"[{row}]");
        }
        w.Indent--;
        w.Line($"];");
    }
}
