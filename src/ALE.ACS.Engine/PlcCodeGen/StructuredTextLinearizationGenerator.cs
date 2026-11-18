using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Linearization;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.PlcCodeGen;

/// <summary>
/// Gera um FUNCTION_BLOCK de Linearização por Realimentação em Structured Text
/// a partir de uma <see cref="LinearizationSpec"/>.
/// </summary>
/// <remarks>
/// A linearização por realimentação aplica a lei u = (v - f(x)) / g(x), onde v
/// é a nova entrada (geralmente saída de um PID externo sobre y - SP). As funções
/// f e g são delegates em C# e não podem ser serializadas diretamente em ST;
/// este gerador emite a <b>estrutura</b> do bloco (interface, estado, ponto nominal,
/// lei de controle paramétrica) e marca claramente os pontos onde o usuário deve
/// inserir as expressões concretas de f(x) e g(x) para o processo específico.
/// </remarks>
public sealed class StructuredTextLinearizationGenerator : IPlcCodeGenerator
{
    public ControllerKind Supports => ControllerKind.Linearization;

    public PlcGenerationResult Generate(PlcGenerationRequest request)
    {
        if (request.Spec is not LinearizationSpec spec)
            return PlcGenerationResult.Fail("Spec não é LinearizationSpec.", request.Target);

        var w = new StructuredTextWriter();
        var blockName = $"{request.BlockPrefix}_LIN_{StructuredTextWriter.Id(spec.Name)}";

        var stateDim = spec.NominalState?.Length ?? 1;
        var mode = spec.Mode;

        if (request.EmitComments)
        {
            w.CommentBlock($"FUNCTION_BLOCK {blockName}");
            w.Comment($"Linearização por realimentação — {spec.Name}");
            w.Comment($"Modo: {mode}");
            w.Comment($"Dimensão do estado: n={stateDim}");
            w.Comment($"InputGain (v→u): {spec.InputGain:G}");
            w.Comment($"OutMin={spec.OutMin:G}, OutMax={spec.OutMax:G}");
            w.Comment($"Scan={StructuredTextWriter.T(spec.ScanTime)}");
            w.Comment("ATENÇÃO: f(x) e g(x) são delegates em C# e NÃO podem ser");
            w.Comment("serializados diretamente. Substitua os placeholders fx_expr e gx_expr");
            w.Comment("pelas expressões concretas do seu processo antes de carregar no PLC.");
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
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "v", "LREAL", "0.0"));
        w.Comment("v = nova entrada (saída do PID externo sobre y-SP)");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Auto", "BOOL", "FALSE"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ManualValue", "LREAL",
            StructuredTextWriter.F(spec.OutMin)));
        w.CloseBlock("END_VAR");
        w.Blank();

        // Estado x (vetor de dimensão n)
        w.OpenBlock("VAR_INPUT");
        w.Comment("Estado x[n] — o caller deve alimentar a cada scan");
        w.Line($"x : {PlcPlatformQuirks.ArraySyntax(request.Target, 0, stateDim - 1, "LREAL")};");
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR_OUTPUT
        w.OpenBlock("VAR_OUTPUT");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, spec.Manipulated.Name, "LREAL",
            StructuredTextWriter.F(spec.Manipulated.Nominal ?? spec.Manipulated.Min)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "Status", "INT", "0"));
        w.CloseBlock("END_VAR");
        w.Blank();

        // VAR
        w.OpenBlock("VAR");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "InputGain", "LREAL", StructuredTextWriter.F(spec.InputGain)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "OutMin", "LREAL", StructuredTextWriter.F(spec.OutMin)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "OutMax", "LREAL", StructuredTextWriter.F(spec.OutMax)));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "ScanTime", "TIME", StructuredTextWriter.T(spec.ScanTime)));
        w.Blank();
        w.Comment("Resultado das funções f(x) e g(x)");
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "fx", "LREAL", "0.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "gx", "LREAL", "1.0"));
        w.Line(PlcPlatformQuirks.VarDecl(request.Target, "u_raw", "LREAL", "0.0"));
        w.CloseBlock("END_VAR");
        w.Blank();

        var mvId = StructuredTextWriter.Id(spec.Manipulated.Name);

        // Run
        w.Line("METHOD PUBLIC Run : LREAL");
        w.OpenBlock("");
        w.Indent--;

        w.Comment("===== PASSO 1: avaliar f(x) e g(x) no ponto atual =====");
        w.Comment("SUBSTITUA os placeholders abaixo pelas expressões concretas");
        w.Comment("do seu processo. Originalmente são delegates C# (F e G).");
        w.Line("fx := 0.0;  (* TODO: substituir por f(x) concreta *)");
        w.Line("gx := 1.0;  (* TODO: substituir por g(x) concreta (deve ser ≠ 0) *)");
        w.Blank();

        w.Comment("===== PASSO 2: lei de linearização u = (v - f(x)) / g(x) =====");
        w.Line("IF gx = 0.0 THEN");
        w.Indent++;
        w.Line("u_raw := ManualValue;  // g(x)=0 impede linearização, fallback seguro");
        w.Indent--;
        w.Line("ELSE");
        w.Indent++;
        w.Line("u_raw := (InputGain * v - fx) / gx;");
        w.Indent--;
        w.Line("END_IF;");
        w.Blank();

        w.Comment("===== PASSO 3: saturação =====");
        w.Line("IF u_raw < OutMin THEN u_raw := OutMin; END_IF;");
        w.Line("IF u_raw > OutMax THEN u_raw := OutMax; END_IF;");
        w.Blank();

        w.Comment("Auto/Manual");
        w.Line("IF Auto THEN");
        w.Indent++; w.Line($"{mvId} := u_raw;"); w.Indent--;
        w.Line("ELSE");
        w.Indent++; w.Line($"{mvId} := ManualValue;"); w.Indent--;
        w.Line("END_IF;");
        w.Blank();
        w.Line("Run := {mvId};");
        w.CloseBlock("END_METHOD");
        w.Blank();

        w.Line("METHOD PUBLIC Reset");
        w.OpenBlock("");
        w.Indent--;
        w.Line("fx := 0.0; gx := 1.0; u_raw := 0.0;");
        w.CloseBlock("END_METHOD");

        w.CloseBlock(PlcPlatformQuirks.EndBlockKeyword(request.Target, asProgram: false));

        var warnings = new List<string>
        {
            "f(x) e g(x) são delegates C# e foram emitidos como placeholders.",
            "Substitua fx e gx pelas expressões concretas do processo antes de carregar no PLC.",
            $"Modo {mode}: a validação de linearizabilidade (Lg h ≠ 0) foi feita em design offline."
        };

        return PlcGenerationResult.Ok(blockName, w.Build(), request.Target, warnings);
    }
}
