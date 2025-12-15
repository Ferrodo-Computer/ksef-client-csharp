using KSeF.Client.Api.Services;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models.QRCode;
using KSeF.Client.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace KSeF.DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class QrCodeController(
    IVerificationLinkService linkSvc) : ControllerBase
{
    private readonly IVerificationLinkService linkSvc = linkSvc ?? throw new ArgumentNullException(nameof(linkSvc));

    // 1. Faktura z numerem KSeF (online)
    [HttpGet("invoice/ksef")]
    public ActionResult<QrCodeResult> GetInvoiceQrWithKsef(
        string nip,
        DateTime issueDate,
        string invoiceHash,
        string ksefNumber)
    {
        string url = linkSvc.BuildInvoiceVerificationUrl(nip, issueDate, invoiceHash);
        byte[] qrCode = QrCodeService.GenerateQrCode(url);
        byte[] labeledQr = QrCodeService.AddLabelToQrCode(qrCode, ksefNumber);

        return Ok(new QrCodeResult(url, Convert.ToBase64String(labeledQr)));
    }

    // 2. Faktura offline (przed wysyłką)
    [HttpGet("invoice/offline")]
    public ActionResult<QrCodeResult> GetInvoiceQrOffline(
        string nip,
        DateTime issueDate,
        string invoiceHash)
    {
        string url = linkSvc.BuildInvoiceVerificationUrl(nip, issueDate, invoiceHash);
        byte[] qrCode = QrCodeService.GenerateQrCode(url);
        byte[] labeledQr = QrCodeService.AddLabelToQrCode(qrCode, "OFFLINE");

        return Ok(new QrCodeResult(url, Convert.ToBase64String(labeledQr)));
    }

    // 3. Weryfikacja certyfikatu (Kod II)
    [HttpGet("certificate")]
    public ActionResult<QrCodeResult> GetCertificateQr(
        string sellerNip,
        QRCodeContextIdentifierType contextIdentifierType,
        string contextIdentifierValue,
        string certSerial,
        string invoiceHash,
        string certbase64,
        string privateKey = ""        
       )
    {
        byte[] bytes = Convert.FromBase64String(certbase64);
        X509Certificate2 certificate = bytes.LoadPkcs12();
        string url = linkSvc.BuildCertificateVerificationUrl(sellerNip,contextIdentifierType ,contextIdentifierValue ,certSerial, invoiceHash, certificate, privateKey);
        byte[] qrCode = QrCodeService.GenerateQrCode(url);
        byte[] labeledQr = QrCodeService.AddLabelToQrCode(qrCode, "CERTYFIKAT");

        return Ok(new QrCodeResult(url, Convert.ToBase64String(labeledQr)));
    }
}
