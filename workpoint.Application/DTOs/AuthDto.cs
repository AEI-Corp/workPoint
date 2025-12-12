namespace workpoint.Application.DTOs;

public class UserRegisterResponseDto
{
    public int Id { get; set; }
    
    public string UserName { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    
    public string NumDocument { get; set; }
    public int DocumentTypeId { get; set; }
    public int Role { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


public class UserAuthResponseDto
{
    public int Id { get; set; }
    
    public string UserName { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public int DocumentTypeId { get; set; }
    public string NumDocument { get; set; }
    public int Role { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


public class RegisterDto
{
    public string UserName { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
    public int DocumentTypeId { get; set; }
    public string NumDocument { get; set; }
    public int RoleId { get; set; }
}

public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RefreshDto
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}

public class RevokeTokenDto
{
    public string Email { get; set; }
}