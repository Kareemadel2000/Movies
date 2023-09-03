using DotNetCore5CRUD.Models;
using DotNetCore5CRUD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCore5CRUD.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDBContext _Context;
        private readonly IToastNotification _toastNotification;
        private List<string> _allowedExtenction = new List<string> { ".jpg", ".png" };
        private long _maxAllowePosterSize = 1048576;
        public MoviesController(ApplicationDBContext Context , IToastNotification toastNotification)
        {
            _Context = Context;
            _toastNotification = toastNotification;
        }
        #region Index
        public async Task<IActionResult> Index()
        {
            var movie = await _Context.Movies.OrderByDescending(m=>m.Rate).ToListAsync();
            return View(movie);
        }
        #endregion

        #region Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync()
            };
            return View("MovieForm", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("MovieForm", model);
            }

            var files = Request.Form.Files;

            if (!files.Any())
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please Select Movie Poster!");
                return View("MovieForm", model);
            }
            var poster = files.FirstOrDefault();


            if (!_allowedExtenction.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Only .PNG , .JPG are allowed!");
                return View("MovieForm", model);
            }

            if (poster.Length > _maxAllowePosterSize)
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                return View("MovieForm", model);
            }

            using var dataStream = new MemoryStream();
            await poster.CopyToAsync(dataStream);

            var movie = new Movies
            {
                Title = model.Title,
                GenreId = model.GenreId,
                Year = model.Year,
                Rate = model.Rate,
                StoryLine = model.StoryLine,
                Poster = dataStream.ToArray()
            };


            _Context.Movies.Add(movie);
            _Context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie Created Successfully");
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _Context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            var viewModel = new MovieFormViewModel
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                Title = movie.Title,
                Poster = movie.Poster,
                Year = movie.Year,
                Rate = movie.Rate,
                StoryLine = movie.StoryLine,
                Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync()
            };
            return View("MovieForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("MovieForm", model);

            }

            var movie = await _Context.Movies.FindAsync(model.Id);

            if (movie == null)
                return NotFound();

            var files = Request.Form.Files;
            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                using var dataStrem = new MemoryStream();
                await poster.CopyToAsync(dataStrem);

                model.Poster = dataStrem.ToArray();

                if (!_allowedExtenction.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Only .PNG , .JPG are allowed!");
                    return View("MovieForm", model);
                }

                if (poster.Length > _maxAllowePosterSize)
                {
                    model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                    return View("MovieForm", model);
                }
                movie.Poster = model.Poster;
            }


            movie.Title = model.Title;
            movie.GenreId = model.GenreId;
            movie.Year = model.Year;
            movie.Rate = model.Rate;
            movie.StoryLine = model.StoryLine;

            _Context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Movie Updated Successfully");
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();
            var movie =await _Context.Movies.Include(m=>m.Genre).SingleOrDefaultAsync(m=>m.Id==id);
            if (movie == null)
                return NotFound();
            return View(movie);
        }
        #endregion


        #region Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var movie = await _Context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();
            _Context.Movies.Remove(movie);
            _Context.SaveChanges();
            return Ok();
        }
        #endregion
    }
}
