# TourApp - Kompletna Implementacija

## Pregled

TourApp je web aplikacija za upravljanje turističkim turama implementirana koristeći:
- **Backend**: .NET Core, ASP.NET Core, Entity Framework Core, PostgreSQL
- **Frontend**: Angular, Angular Material, Leaflet za mape
- **Arhitektura**: DDD (Domain-Driven Design), Clean Architecture
- **Autentifikacija**: JWT tokens
- **Email**: SMTP sa Gmail

## Implementirane funkcionalnosti

### ✅ **Osnovni zahtevi**
- ✅ **.NET Core + Angular** - Implementirano
- ✅ **Clean Code principi** - Implementirano (DDD arhitektura)
- ✅ **Scrum metodologija** - Implementirano (inkrementalno razvijanje)
- ✅ **PostgreSQL baza** - Implementirano
- ✅ **DDD principi** - Implementirano (Domain, Application, Infrastructure sloj)

### ✅ **Map funkcionalnost**
- ✅ **Leaflet implementacija** - Implementirano
- ✅ **Interaktivna mapa** sa OpenStreetMap tiles
- ✅ **Ključne tačke** sa custom marker-ima
- ✅ **Putanje između tačaka** sa različitim bojama
- ✅ **Popup informacije** za svaku tačku
- ✅ **Responsive dizajn**

### ✅ **Email funkcionalnost**
- ✅ **SMTP konfiguracija** sa Gmail
- ✅ **Welcome email** za registraciju
- ✅ **Purchase confirmation** email
- ✅ **Tour cancellation** notifications
- ✅ **Problem report** notifications
- ✅ **Block/unblock** notifications
- ✅ **Tour reminders** (48h pre)
- ✅ **Monthly reports** za vodiče
- ✅ **Tour recommendations** po interesovanjima

## Email Konfiguracija

### Trenutna Konfiguracija

**SMTP Server:** smtp.gmail.com  
**Port:** 587  
**Username:** todorovic.1milan@gmail.com  
**From Email:** tourapp@psw.com  
**From Name:** TourApp  

## Pregled

TourApp koristi SMTP za slanje email notifikacija korisnicima. Implementirane su sledeće email funkcionalnosti:

### Email notifikacije:
1. **Welcome Email** - Dobrodošlica novim korisnicima
2. **Block Notification** - Obaveštenje o blokadi naloga
3. **Unblock Notification** - Obaveštenje o odblokiranju naloga
4. **Purchase Confirmation** - Potvrda kupovine ture
5. **Cancellation Notification** - Obaveštenje o otkazivanju ture
6. **Problem Report Notification** - Obaveštenje o prijavljenom problemu

## Konfiguracija

### 1. Gmail SMTP (Preporučeno)

#### Korak 1: Omogućite 2FA na Gmail nalogu
1. Idite na [Google Account Settings](https://myaccount.google.com/)
2. Omogućite 2-Step Verification

#### Korak 2: Generišite App Password
1. Idite na [App Passwords](https://myaccount.google.com/apppasswords)
2. Generišite novi App Password za "Mail"
3. Kopirajte generisani password (16 karaktera)

#### Korak 3: Ažurirajte appsettings.json
```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "todorovic.1milan@gmail.com",
    "Password": "xcye pmvg exqn uclp",
    "FromEmail": "tourapp@psw.com",
    "FromName": "TourApp"
  }
}
```

### 2. Outlook/Hotmail SMTP

```json
{
  "Email": {
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "Username": "your-email@outlook.com",
    "Password": "your-password",
    "FromEmail": "your-email@outlook.com",
    "FromName": "TourApp"
  }
}
```

### 3. Custom SMTP Server

```json
{
  "Email": {
    "SmtpServer": "your-smtp-server.com",
    "SmtpPort": 587,
    "Username": "your-username",
    "Password": "your-password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "TourApp"
  }
}
```

## Testiranje Email Funkcionalnosti

### 1. Test Email Endpoint
Najlakši način da testirate email funkcionalnost je kroz test endpoint:

```bash
curl -X POST "https://localhost:7249/api/Users/test-email" \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com"}'
```

Ili kroz Swagger UI na: `https://localhost:7249/swagger`

### 2. Registracija korisnika
- Kada se korisnik registruje, automatski se šalje welcome email

### 3. Admin funkcionalnosti
- Blokiranje korisnika šalje notification email
- Odblokiranje korisnika šalje notification email

### 4. Turističke funkcionalnosti
- Kupovina ture šalje confirmation email
- Otkazivanje ture šalje notification email svim kupcima
- Prijavljivanje problema šalje notification email

## Troubleshooting

### Problem: "Authentication failed"
- Proverite da li je 2FA omogućen (za Gmail)
- Proverite da li koristite App Password umesto obične lozinke
- Proverite da li su credentials ispravni

### Problem: "Connection timeout"
- Proverite da li je SMTP server ispravan
- Proverite da li je port ispravan
- Proverite firewall/postavke

### Problem: "Email not sending"
- Proverite console logove za detaljne greške
- EmailService neće crash-ovati aplikaciju ako email ne može da se pošalje
- Greške se loguju u console

## Development Setup

Za development, možete koristiti:

1. **Gmail App Password** (preporučeno)
2. **Mailtrap** za testiranje
3. **Fake SMTP server** kao MailHog

### Mailtrap Setup
```json
{
  "Email": {
    "SmtpServer": "smtp.mailtrap.io",
    "SmtpPort": 2525,
    "Username": "your-mailtrap-username",
    "Password": "your-mailtrap-password",
    "FromEmail": "noreply@tourapp.com",
    "FromName": "TourApp"
  }
}
```

## Security Notes

1. **Nikad ne commit-ujte prave credentials** u git
2. Koristite **App Passwords** umesto običnih lozinki
3. Koristite **Environment Variables** za production
4. **EmailService** je dizajniran da ne crash-uje aplikaciju ako email ne može da se pošalje

## Production Deployment

Za production, koristite environment variables:

```bash
export Email__SmtpServer="smtp.gmail.com"
export Email__SmtpPort="587"
export Email__Username="todorovic.1milan@gmail.com"
export Email__Password="xcye pmvg exqn uclp"
export Email__FromEmail="tourapp@psw.com"
export Email__FromName="TourApp"
``` 