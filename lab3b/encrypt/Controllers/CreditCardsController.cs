using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using encrypt.Data;
using encrypt.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace encrypt.Controllers
{
    [Authorize]
    public class CreditCardsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CreditCardsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: CreditCards
        public async Task<IActionResult> Index()
        {
            return View(await _context.CreditCard.ToListAsync());
        }

        // GET: CreditCards/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var creditCard = await _context.CreditCard.SingleOrDefaultAsync(m => m.Id == id);
                if (creditCard == null)
                {
                    return NotFound();
                }

                string hmacKey = _configuration.GetSection("Keys").GetValue<string>("L3B-HMACSK");

                if (creditCard.SECC == hmacKey)
                {
                    creditCard.PTCC = Decrypt(creditCard.ECC);
                    creditCard.PTCVC = Decrypt(creditCard.ECVC);

                    return View(creditCard);
                }
                else
                {
                    return View();
                }
            }
            catch
            {
                this.ModelState.AddModelError("", "Unable to view credit card");

                return View();
            }
        }

        // GET: CreditCards/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CreditCards/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreditCard creditCard)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    return View(creditCard);
                }

                string hmacKey = _configuration.GetSection("Keys").GetValue<string>("L3B-HMACSK");


                creditCard.Id = Guid.NewGuid();
                creditCard.ECC = Encrypt(creditCard.PTCC);
                creditCard.ECVC = Encrypt(creditCard.PTCVC);
                creditCard.SECC = hmacKey;

                _context.Add(creditCard);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = creditCard.Id });
            }
            catch
            {
                this.ModelState.AddModelError("", "Unable to create credit card");

                return View();
            }
        }

        private string Encrypt(string content)
        {
            string aesKey = _configuration.GetSection("Keys").GetValue<string>("L3B-AKEY");
            string aesIv = _configuration.GetSection("Keys").GetValue<string>("L3B-AIV");
            //string hmacKey = _configuration.GetValue<string>("L3B-HMACSK");

            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(aesKey);

                aes.IV = Convert.FromBase64String(aesIv);

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                var input = Encoding.UTF8.GetBytes(content);

                var encryptedContent = encryptor.TransformFinalBlock(input, 0, input.Length);

                // convert the encrypted content to a base 64 encoded string
                return Convert.ToBase64String(encryptedContent);
            }
        }

        private string Decrypt(string content)
        {
            string aesKey = _configuration.GetSection("Keys").GetValue<string>("L3B-AKEY");
            string aesIv = _configuration.GetSection("Keys").GetValue<string>("L3B-AIV");

            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(aesKey);

                aes.IV = Convert.FromBase64String(aesIv);

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                var input = Convert.FromBase64String(content);

                var decryptedContent = decryptor.TransformFinalBlock(input, 0, input.Length);

                // convert the result back to a human readable string
                return Encoding.UTF8.GetString(decryptedContent);
            }
        }

        // GET: CreditCards/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var creditCard = await _context.CreditCard.SingleOrDefaultAsync(m => m.Id == id);
            if (creditCard == null)
            {
                return NotFound();
            }
            return View(creditCard);
        }

        // POST: CreditCards/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ECC,SECC")] CreditCard creditCard)
        {
            if (id != creditCard.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(creditCard);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CreditCardExists(creditCard.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(creditCard);
        }

        // GET: CreditCards/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var creditCard = await _context.CreditCard.SingleOrDefaultAsync(m => m.Id == id);
            if (creditCard == null)
            {
                return NotFound();
            }

            string hmacKey = _configuration.GetSection("Keys").GetValue<string>("L3B-HMACSK");

            if (creditCard.SECC == hmacKey)
            {
                creditCard.PTCC = Decrypt(creditCard.ECC);
            }

            return View(creditCard);
        }

        // POST: CreditCards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var creditCard = await _context.CreditCard.SingleOrDefaultAsync(m => m.Id == id);
            _context.CreditCard.Remove(creditCard);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CreditCardExists(Guid id)
        {
            return _context.CreditCard.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Keygen(string cc)
        {
            keygen kg = new keygen();
            var hmacKey = new byte[64];

            using (var rngProvider = new RNGCryptoServiceProvider())
            {
                rngProvider.GetBytes(hmacKey);
            }

            kg.hmacKey = Convert.ToBase64String(hmacKey);

            using (var aes = Aes.Create())
            {
                kg.AesKey = Convert.ToBase64String(aes.Key);
                kg.AesIv = Convert.ToBase64String(aes.IV);
            }

            return Ok(kg);
        }
    }

    public class keygen
    {
        public string hmacKey { set; get; }
        public string AesKey { set; get; }
        public string AesIv { set; get; }
        public keygen()
        {

        }
    }
}