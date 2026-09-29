namespace FinTrack.Application.Common;

public class NotFoundException(string message) : Exception(message);
public class ConflictException(string message) : Exception(message);
public class AuthenticationException() : Exception("Email ou senha inválidos.");
