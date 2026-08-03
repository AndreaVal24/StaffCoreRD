using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StaffCoreRD.Models;

namespace StaffCoreRD.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // REGISTRO: GET
        public IActionResult Register()
        {
            return View();
        }

        // REGISTRO: POST - AHORA RECIBE RegisterViewModel EN LUGAR DE STRINGS
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Validar ModelState: aquí es donde se validan los atributos [Required], [Compare], etc
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Crear usuario con email como nombre de usuario
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Si es primer usuario → Admin, si no → Viewer
                var userCount = _userManager.Users.Count();

                if (userCount == 1)
                    await _userManager.AddToRoleAsync(user, "Administrador");
                else if (userCount == 2)
                    await _userManager.AddToRoleAsync(user, "RRHH");
                else
                    await _userManager.AddToRoleAsync(user, "Viewer");


                // Iniciar sesión automáticamente
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }

            // Si falla, agregar errores al ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // LOGIN: GET
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN: POST - AHORA RECIBE LoginViewModel EN LUGAR DE STRINGS
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Validar ModelState: aquí se validan [Required], [EmailAddress], etc
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Intentar iniciar sesión con lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            // Si la cuenta está bloqueada
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "La cuenta está bloqueada por intentos fallidos. Intenta más tarde.");
            }
            else
            {
                // Credenciales incorrectas
                ModelState.AddModelError("", "Email o contraseña incorrectos.");
            }

            return View(model);
        }

        // LOGOUT: POST
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}