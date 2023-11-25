using System.Collections;
using Compilador.Graph;
using Compilador.Processors.Parser;

namespace Compilador.Quackier;
public class TypeChecker
{
    private const string DECLARATION = "DECLARATION";
    private const string ASSIGNMENT = "ASSIGNMENT";
    private const string EXPRESSION = "EXPRESSION";
    private const string TERM = "TERM";
    private const string VALUE = "VALUE";
    private const string STRING = "string";
    private const string REAL = "number_type";
    private const string ID = "id";

    private Dictionary<string, int> symbolIndex;
    private Tree tree;
    private SymbolTable symbolTable;

    public TypeChecker(Tree tree, ParserSetup setup)
    {
        this.tree = tree;
        symbolIndex = new Dictionary<string, int>()
        {
            {DECLARATION, setup.GetIndexOf(DECLARATION)},
            {ASSIGNMENT, setup.GetIndexOf(ASSIGNMENT)},
            {EXPRESSION, setup.GetIndexOf(EXPRESSION)},
            {VALUE, setup.GetIndexOf(VALUE)},
            {TERM, setup.GetIndexOf(TERM)},
            {STRING, setup.GetIndexOf(STRING)},
            {REAL, setup.GetIndexOf(REAL)},
            {ID, setup.GetIndexOf(ID)}
        };
        symbolTable = new SymbolTable();
    }

    public bool CheckTypes()
    {
        symbolTable = new SymbolTable();
        SetDeclarations();

        var levels = tree.GetTreeLevels();
        levels.Reverse();

        foreach (var level in levels)
        {
            foreach (var node in level)
            {
                SetType(node);
            }
        }

        CheckDeclarations();
        return true;
    }

    private void CheckDeclarations()
    {
        foreach (var node in tree.CalculatePostorder())
        {
            if (node.Value == symbolIndex[DECLARATION])
            {
                var idType = GetIdType(node, 1);
                var valueType = GetSymbolType(node, 3);

                if (idType != valueType)
                    throw new Exception($"Error: Cannot assign {idType} to a {valueType} variable: Variable {node.Children[1].Data}");
            }
        }
    }

    private void SetDeclarations()
    {
        List<int> variables = new List<int>();
        foreach (var node in tree.CalculatePostorder())
        {
            if (node.Value == symbolIndex[DECLARATION])
            {
                var type = node.Children[0].Value == symbolIndex[REAL] ?
                    SymbolType.Real: SymbolType.String;
                var id = node.Children[1].Data.GetHashCode();

                if (variables.Contains(id))
                    throw new Exception("Variable already declared");
                else
                    variables.Add(id);

                var value = node.Children[2].Data;
                symbolTable.Bind(id, new Symbol(type, value));
            }
        }
    }

    private void SetType(SimpleNode node)
    {
        if (node.Value == symbolIndex[ASSIGNMENT])
        {
            var idType = GetIdType(node, 0);
            var valueType = GetSymbolType(node, 2);

            if (idType != valueType)
                throw new Exception($"Error: Cannot assign {valueType} to a {idType} variable");
        }
        else if (node.Value == symbolIndex[EXPRESSION])
        {
            if (node.Children.Count == 3)
            {
                var expType = GetSymbolType(node, 0);
                var termType = GetSymbolType(node, 2);

                if (expType != termType)
                    throw new Exception($"Error: Cannot operate {expType} with a {termType}");
                else
                    symbolTable.Bind(node.GetHashCode(), new Symbol(expType, 0));
            }
            else
            {
                var termType = GetSymbolType(node, 0);
                symbolTable.Bind(node.GetHashCode(), new Symbol(termType, 0));
            }
        }
        else if (node.Value == symbolIndex[TERM])
        {
            SymbolType type = SymbolType.Real;
            if (node.Children.Count == 3)
                type = GetSymbolType(node, 1);
            else if (node.Children[0].Value == symbolIndex[ID])
                type = GetIdType(node, 0);

            symbolTable.Bind(node.GetHashCode(), new Symbol(type, node.Data));
        }
        else if (node.Value == symbolIndex[VALUE])
        {
            SymbolType type = SymbolType.Real;
            if (node.Children[0].Value == symbolIndex[ID])
                type = GetIdType(node, 0);
            else if (node.Children[0].Value == symbolIndex[STRING])
                type = SymbolType.String;
            else if (node.Children[0].Value == symbolIndex[EXPRESSION])
                type = GetSymbolType(node, 0);

            symbolTable.Bind(node.GetHashCode(), new Symbol(type, node.Data));
        }
        else if (node.Value == symbolIndex[STRING])
            symbolTable.Bind(node.GetHashCode(), new Symbol(SymbolType.String, node.Data));
        else
            symbolTable.Bind(node.GetHashCode(), new Symbol(SymbolType.Real, node.Data));
    }

    private SymbolType GetSymbolType(SimpleNode node, int childIndex)
    {
        var symbol = node.Children[childIndex].GetHashCode();
        var type = symbolTable.Lookup(symbol)?.Type;

        if (type == null)
            throw new Exception($"Error in type checking: {node.Children[childIndex].Data} not declared");

        return (SymbolType)type;
    }

    private SymbolType GetIdType(SimpleNode node, int childIndex)
    {
        var symbol = node.Children[childIndex].Data.GetHashCode();
        var type = symbolTable.Lookup(symbol)?.Type;

        if (type == null)
            throw new Exception($"Variable {node.Children[childIndex].Data} not declared");

        return (SymbolType)type;
    }
}