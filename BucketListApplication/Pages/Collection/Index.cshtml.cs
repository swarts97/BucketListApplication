﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BucketListApplication.Models;
using BucketListApplication.Data;
using BucketListApplication.Models.BLViewModels;

namespace BucketListApplication.Pages.Collection
{
    public class IndexModel : PageModel
    {
        private readonly BLContext _context;

        public IndexModel(BLContext context)
        {
            _context = context;
        }

        public CategoryIndexData CategoryData { get; set; }
        public int CategoryID { get; set; }

        public async Task OnGetAsync(int? categoryId)
        {
			CategoryData = new CategoryIndexData
			{
				Categories = await _context.Categories
                    .AsNoTracking()
                    .Include(c => c.ElementCategories)
				        .ThenInclude(ec => ec.Element)
				    .OrderBy(c => c.Name)
				    .ToListAsync()
			};

			if (categoryId != null)
            {
                CategoryID = categoryId.Value;
                Category category = CategoryData.Categories
                    .Where(c => c.CategoryID == categoryId.Value)
                    .Single();
                CategoryData.Elements = category.ElementCategories
                    .Select(ec => ec.Element)
                    .Where(ec => ec.Discriminator == "Element")
                    .OrderBy(e => e.Name);
            }
        }
    }
}
