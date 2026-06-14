namespace Store.Store;

public class EmptyArgumentException(string paramName) : ArgumentException("Argument is null or empty", paramName) { }
