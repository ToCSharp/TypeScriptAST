# TypeScriptAST
.NET port of Microsoft's TypeScript parser for simple AST manipulation.

If works with TypeScript, JavaScript and DefinitelyTyped(".d.ts") files and gives the same tree as typescriptServices.js.

## TS file modification Example

This example included in TypeScriptAstExample. It finds modules in file, collects some info, adds new function to module.

```csharp
  using Zu.TypeScript;
  using Zu.TypeScript.Change;
  using Zu.TypeScript.TsTypes;
  
            var fileName = "parser.ts";
            var source = File.ReadAllText(fileName);

            var ast = new TypeScriptAST(source, fileName);

            var change = new ChangeAST();

            foreach (var module in ast.GetDescendants().OfType<ModuleDeclaration>())
            {
                var funcs = module.Body.Children.OfType<FunctionDeclaration>().ToList();
                var enums = module.Body.Children.OfType<EnumDeclaration>();
                var moduleInfoFunc = $@"
    export function getModuleInfo() {{
        return ""Module {module.IdentifierStr} contains {funcs.Count()} functions ({funcs.Count(v => v.IdentifierStr.StartsWith("parse"))} starts with parse), {enums.Count()} enums ..."";
    }}
";
                change.InsertBefore(module.Body.Children.First(), moduleInfoFunc);
            }
            var newSource = change.GetChangedSource(ast.SourceStr);

            File.WriteAllText("parser2.ts", newSource);

```
