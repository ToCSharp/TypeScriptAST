# TypeScriptAST
.NET port of Microsoft's TypeScript parser for simple AST manipulation.

It works with TypeScript, JavaScript and DefinitelyTyped(".d.ts") files and gives the same tree as typescriptServices.js.

## Create AST
```csharp
var ast = new TypeScriptAST(File.ReadAllText(file), file);
```
## Find Node
By SyntaxKind
```csharp
  var functions = ast.OfKind(SyntaxKind.FunctionExpression);
  var vars = functions.FirstOrDefault()?.OfKind(SyntaxKind.VariableDeclaration);
```
By Node type
```csharp
  var functions = ast.GetDescendants().OfType<FunctionExpression>();
  var vars = functions.FirstOrDefault()?.GetDescendants().OfType<VariableDeclaration>();
```
## GetText
```csharp
  var firstFunc = ast.OfKind(SyntaxKind.FunctionExpression).FirstOrDefault();
  var text = firstFunc?.GetText();
  var withComments = firstFunc?.GetTextWithComments();
```
## Change Node
```csharp
  var funcNewCode = "function() {}";
  var change = new ChangeAST();
  change.ChangeNode(firstFunc, funcNewCode);
  var newSource = change.GetChangedSource(ast.SourceStr);
  File.WriteAllText(file, newSource);
```
## File modification Example

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
