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
        public MoviesController(ApplicationDBContext Context )
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
                Genres = await _Context.Genres.ToListAsync()
            };
            return View(viewModel);
        }
    }
}
