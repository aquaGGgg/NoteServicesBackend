namespace Notes.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int UserId { get; } // valid only when authenticated
}
//блядский хелпер проверики булом авторизации юзера