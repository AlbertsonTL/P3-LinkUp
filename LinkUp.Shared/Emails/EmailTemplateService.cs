namespace LinkUp.Shared.Emails;

// Genera plantillas HTML con el diseño de LinkUp para los correos transaccionales.
public static class EmailTemplateService
{
    private static string Base(string preheader, string bodyContent, string? logoUrl = null) => $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
  <title>LinkUp</title>
  <!--[if mso]>
  <noscript><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch></o:OfficeDocumentSettings></xml></noscript>
  <![endif]-->
  <style>
    * {{ margin:0; padding:0; box-sizing:border-box; }}
    body {{
      background:#F0F3FA;
      font-family: 'Segoe UI', Arial, sans-serif;
      color:#1A2540;
      -webkit-font-smoothing:antialiased;
    }}
    a {{ color:#0080A0; text-decoration:none; }}
    a:hover {{ text-decoration:underline; }}
    .email-wrapper {{
      max-width:600px;
      margin:40px auto;
      background:#ffffff;
      border-radius:16px;
      overflow:hidden;
      box-shadow:0 8px 40px rgba(0,32,96,.12);
    }}
    /* Header */
    .email-header {{
      background:linear-gradient(135deg, #001440 0%, #002060 55%, #003080 100%);
      padding:32px 40px 28px;
      text-align:center;
    }}
    .email-header h1 {{
      color:#ffffff;
      font-size:26px;
      font-weight:800;
      letter-spacing:-0.5px;
      margin:0;
    }}
    .email-header p {{
      color:rgba(255,255,255,.65);
      font-size:13px;
      margin-top:6px;
    }}
    /* Body */
    .email-body {{
      padding:40px 40px 32px;
    }}
    .email-body h2 {{
      font-size:22px;
      font-weight:800;
      color:#002060;
      margin-bottom:12px;
    }}
    .email-body p {{
      font-size:15px;
      line-height:1.7;
      color:#445070;
      margin-bottom:16px;
    }}
    .email-body p strong {{ color:#1A2540; }}
    /* CTA Button */
    .btn-cta {{
      display:inline-block;
      background:linear-gradient(135deg, #002060 0%, #003080 100%);
      color:#ffffff !important;
      font-size:15px;
      font-weight:700;
      padding:14px 36px;
      border-radius:50px;
      text-decoration:none !important;
      letter-spacing:0.3px;
      box-shadow:0 6px 20px rgba(0,32,96,.30);
      transition:all .2s;
      margin:8px 0 24px;
    }}
    .btn-cta-gold {{
      background:linear-gradient(135deg, #D09030 0%, #E0A040 100%);
      color:#001440 !important;
      box-shadow:0 6px 20px rgba(224,160,64,.35);
    }}
    .btn-wrapper {{ text-align:center; margin:28px 0; }}
    /* Info box */
    .info-box {{
      background:#F0F3FA;
      border-left:4px solid #E0A040;
      border-radius:0 8px 8px 0;
      padding:14px 18px;
      margin:20px 0;
      font-size:14px;
      color:#445070;
      line-height:1.6;
    }}
    .info-box.teal {{ border-left-color:#0080A0; }}
    .info-box.danger {{ border-left-color:#DC3545; background:#FEF0F0; }}
    /* Divider */
    .divider {{
      border:none;
      border-top:1px solid #E8ECEF;
      margin:24px 0;
    }}
    /* Link fallback */
    .link-fallback {{
      font-size:12px;
      color:#8894AC;
      word-break:break-all;
      background:#F7F9FC;
      border:1px solid #E2E8F0;
      border-radius:8px;
      padding:10px 14px;
      margin-top:8px;
      display:block;
    }}
    /* Footer */
    .email-footer {{
      background:#F7F9FC;
      border-top:1px solid #E8ECEF;
      padding:24px 40px;
      text-align:center;
    }}
    .email-footer p {{
      font-size:12px;
      color:#8894AC;
      line-height:1.6;
      margin-bottom:4px;
    }}
    .email-footer .footer-brand {{
      font-weight:700;
      color:#002060;
    }}
    @media (max-width:600px) {{
      .email-wrapper {{ margin:0; border-radius:0; }}
      .email-body, .email-footer {{ padding:28px 24px; }}
      .email-header {{ padding:24px; }}
    }}
  </style>
</head>
<body>
  <!-- Preheader invisible -->
  <div style=""display:none;max-height:0;overflow:hidden;mso-hide:all;font-size:1px;color:#F0F3FA"">
    {preheader}&nbsp;&#847;&nbsp;&#847;&nbsp;&#847;&nbsp;&#847;&nbsp;&#847;&nbsp;&#847;&nbsp;&#847;
  </div>

  <div style=""padding:20px 16px"">
    <div class=""email-wrapper"">

      <!-- Header -->
      <div class=""email-header"">
        <h1>LinkUp</h1>
        <p>Tu red social</p>
      </div>

      <!-- Body -->
      <div class=""email-body"">
        {bodyContent}
      </div>

      <!-- Footer -->
      <div class=""email-footer"">
        <p>Este correo fue enviado automáticamente por <span class=""footer-brand"">LinkUp</span>. Por favor no respondas.</p>
        <p>Si no solicitaste esta acción, puedes ignorar este mensaje.</p>
        <p style=""margin-top:12px"">
          <span class=""footer-brand"">LinkUp</span> &copy; {DateTime.UtcNow.Year} — Todos los derechos reservados
        </p>
      </div>

    </div>
  </div>
</body>
</html>";

    // 1. Activación de cuenta
    public static string AccountActivation(string firstName, string activationLink) =>
        Base(
            preheader: $"Hola {firstName}, activa tu cuenta para empezar a usar LinkUp.",
            bodyContent: $@"
        <h2>¡Bienvenido/a, {firstName}! 🎉</h2>
        <p>Gracias por registrarte en <strong>LinkUp</strong>. Ya casi estás listo/a para conectar con tus amigos, compartir publicaciones y jugar Battleship.</p>
        <p>Solo necesitas activar tu cuenta haciendo clic en el botón de abajo:</p>

        <div class=""btn-wrapper"">
          <a href=""{activationLink}"" class=""btn-cta btn-cta-gold"" target=""_blank"">
            ✓ &nbsp; Activar mi cuenta
          </a>
        </div>

        <div class=""info-box"">
          <strong>⚠️ Importante:</strong> Este enlace es válido por <strong>24 horas</strong>.
          Si no activas tu cuenta en ese tiempo, deberás registrarte nuevamente.
        </div>

        <hr class=""divider"" />
        <p style=""font-size:13px;color:#8894AC"">Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
        <span class=""link-fallback"">{activationLink}</span>"
        );

    // 2. Restablecimiento de contraseña
    public static string PasswordReset(string firstName, string resetLink) =>
        Base(
            preheader: $"Hola {firstName}, recibimos una solicitud para restablecer tu contraseña.",
            bodyContent: $@"
        <h2>Restablecer contraseña 🔑</h2>
        <p>Hola <strong>{firstName}</strong>, recibimos una solicitud para restablecer la contraseña de tu cuenta en LinkUp.</p>
        <p>Si fuiste tú, haz clic en el botón de abajo para elegir una nueva contraseña:</p>

        <div class=""btn-wrapper"">
          <a href=""{resetLink}"" class=""btn-cta"" target=""_blank"">
            🔐 &nbsp; Restablecer contraseña
          </a>
        </div>

        <div class=""info-box teal"">
          <strong>⏱ Enlace con tiempo limitado:</strong> Este enlace expira en <strong>24 horas</strong>.
          Después de ese tiempo deberás solicitar uno nuevo.
        </div>

        <div class=""info-box danger"">
          <strong>🚨 ¿No solicitaste esto?</strong> Si no pediste restablecer tu contraseña,
          ignora este correo. Tu cuenta permanece segura y sin cambios.
        </div>

        <hr class=""divider"" />
        <p style=""font-size:13px;color:#8894AC"">Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
        <span class=""link-fallback"">{resetLink}</span>"
        );
}
