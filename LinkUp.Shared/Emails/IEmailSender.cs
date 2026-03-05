namespace LinkUp.Shared.Emails;

/* Contrato del servicio de correo. La capa Shared define el contrato para que
 Application e Infrastructure lo utilicen e implementen respectivamente */
public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}
