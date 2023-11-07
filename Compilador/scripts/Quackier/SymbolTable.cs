namespace Compilador.Quackier;

/// <summary>
/// The SymbolTable class is responsible for storing the symbols of the input code.
/// </summary>
public class SymbolTable
{
    Stack<Dictionary<int, Symbol>> scopes;
    Dictionary<int, Symbol> symbols => scopes.Peek();

    public SymbolTable()
    {
        scopes = new Stack<Dictionary<int, Symbol>>();
        scopes.Push(new Dictionary<int, Symbol>());
    }

    public void Bind(int hashCode, Symbol symbol)
    {
        if(symbols.ContainsKey(hashCode))
            symbols[hashCode] = symbol;
        else
            symbols.Add(hashCode, symbol);
    }

    public Symbol? Lookup(int hashCode)
    {
        if (symbols.ContainsKey(hashCode))
            return symbols[hashCode];
        else
            return null;
    }

    public void EnterScope()
    {
        scopes.Push(CopySymbols());
    }

    public void ExitScope()
    {
        scopes.Pop();
    }

    private Dictionary<int, Symbol> CopySymbols()
    {
        Dictionary<int, Symbol> copy = new Dictionary<int, Symbol>();
        foreach (KeyValuePair<int, Symbol> pair in symbols)
            copy.Add(pair.Key, pair.Value);
        return copy;
    }
}

public class Symbol
{
    private SymbolType type;
    private object value;

    public Symbol(SymbolType type, object value)
    {
        this.type = type;
        this.value = value;
    }

    public object Value { get => value; set => this.value = value; }
    internal SymbolType Type { get => type; }
}

public enum SymbolType
{
    Real,
    String
}
