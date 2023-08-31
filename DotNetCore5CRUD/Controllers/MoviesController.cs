using DotNetCore5CRUD.Models;
using DotNetCore5CRUD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCore5CRUD.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDBContext _Context;
        public MoviesController(ApplicationDBContext Context)
        {
            _Context = Context;
        }
        public async Task<IActionResult> Index()
        {
            var movie = await _Context.Movies.ToListAsync();
            return View(movie);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync()
            };
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View(model);
            }

            var files = Request.Form.Files;

            if (!files.Any())
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please Select Movie Poster!");
                return View(model);
            }
            //model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
            return View(model);
        }
    }
}
