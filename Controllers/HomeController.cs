using GeraPdf.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;

namespace GeraPdf.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //var caminho = @"E:\Meus documentos\Projetos\MMTech\GeraPdf\pdf\demo.pdf";
            //var pdrfWriter = new PdfWriter(caminho);
            //var pdf = new PdfDocument(pdrfWriter);
            //var documento = new Document(pdf);
            //var cabecalho = new Paragraph("Recibo de entrega de encomendas")
            //    .SetTextAlignment(TextAlignment.CENTER)
            //    .SetFontSize(20);

            //var texto = new Paragraph(@"Declaro que recebi a encomenda entregue pelos correios sob número de registro 962332e7-23a1-43f3-b2a1-85b2f7aa7529 dcb84a73-d96f-4139-b311-dd42b603b22a a17b882d-b76d-403f-a5ae-b170330f1de2 a245da8e-c520-40b0-bc8c-ae1240525f14")
            //    .SetFontSize(12)
            //    .SetTextAlignment(TextAlignment.LEFT)
            //    .SetHeight(300);

            //var qualificacao = new Paragraph(@"Mario Quintana")
            //    .SetFontSize(12)
            //    .SetTextAlignment(TextAlignment.CENTER);

            //var cpf = new Paragraph(@"935.620.400-40")
            //    .SetFontSize(12)
            //    .SetTextAlignment(TextAlignment.CENTER);

            //documento.Add(cabecalho);
            //documento.Add(texto);
            //documento.Add(qualificacao);
            //documento.Add(cpf);
            //documento.Close();


            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult CriarPdf(string nome, string cpf, string local, DateTime data)
        {
            var cpfFormatado = FormatarCpf(cpf);
            using var fileStream = new MemoryStream();
            var pdrfWriter = new PdfWriter(fileStream);
            var pdf = new PdfDocument(pdrfWriter);
            //var documento = new Document(pdf, PageSize.A4.Rotate()); // para página em paisagem 
            var documento = new Document(pdf, PageSize.A4);
            var cabecalho = new Paragraph("Recibo de entrega de encomendas")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(14)
                .SetHeight(50);

            var primeiroParagrafo = new Paragraph($"Eu, {nome}, CPF {cpfFormatado}, declaro que recebi da Empresa Brasileira de Correios e Telégrafos as encomendas relacionadas abaixo:")
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFirstLineIndent(50f)
                .SetHeight(50);

            var tabela = new Table(3)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            var identificacaoHeaderCell = new Cell().Add(new Paragraph("Identificação").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetHorizontalAlignment(HorizontalAlignment.CENTER);
            var notaFiscalHeaderCell = new Cell().Add(new Paragraph("Nota fiscal").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetHorizontalAlignment(HorizontalAlignment.CENTER);
            var valorHeaderCell = new Cell().Add(new Paragraph("Valor").SetBold().SetTextAlignment(TextAlignment.CENTER)).SetHorizontalAlignment(HorizontalAlignment.CENTER);

            tabela.AddHeaderCell(identificacaoHeaderCell)
                  .AddHeaderCell(notaFiscalHeaderCell)
                  .AddHeaderCell(valorHeaderCell);

            var random = new Random();
            var itens = random.Next(1, 11);
            for (int i = 0; i < itens; i++)
            {
                var ident = new Paragraph(Guid.NewGuid().ToString());
                var identCell = new Cell().Add(ident);
                tabela.AddCell(identCell);

                var notaFiscal = new Paragraph(random.Next(1000, 9999999).ToString());
                var notaFiscalCell = new Cell().Add(notaFiscal);
                tabela.AddCell(notaFiscalCell);

                var vlr = Math.Round(random.Next(1000, 1000000) / 100.0, 2);
                var valor = new Paragraph($"{vlr:c}").SetTextAlignment(TextAlignment.RIGHT);
                var valorCell = new Cell().Add(valor);
                tabela.AddCell(valorCell);
            }

            var localEData = new Paragraph(@$"{local}, {data.Day:00} de {ObterMesPorExtenso(data.Month)} de {data.Year}.")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(11)
                .SetPaddingTop(50)
                .SetHeight(100);

            var linhaAssinatura = new Paragraph(new string('_', nome.Length + 10)).SetTextAlignment(TextAlignment.CENTER);

            var nomeAssinatura = new Paragraph(nome)
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER);

            var cpfAssinatura = new Paragraph(cpfFormatado)
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER);

            documento.Add(cabecalho);
            documento.Add(primeiroParagrafo);
            documento.Add(tabela);
            documento.Add(localEData);
            documento.Add(linhaAssinatura);
            documento.Add(nomeAssinatura);
            documento.Add(cpfAssinatura);
            documento.Close();

            return File(fileStream.ToArray(), "application/pdf", @$"Recibo - {nome} - {DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        private string ObterMesPorExtenso(int mes)
        {
            var meses = new Dictionary<int, string>
                {
                    { 1, "Janeiro" },
                    { 2, "Fevereiro" },
                    { 3, "Março" },
                    { 4, "Abril" },
                    { 5, "Maio" },
                    { 6, "Junho" },
                    { 7, "Julho" },
                    { 8, "Agosto" },
                    { 9, "Setembro" },
                    { 10, "Outubro" },
                    { 11, "Novembro" },
                    { 12, "Dezembro" }
                };

            return meses[mes];
        }

        public string FormatarCpf(string cpf)
        {
            return Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");
        }
    }
}
