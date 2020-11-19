﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BucketListApplication.Data;
using BucketListApplication.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BucketListApplication.Pages.BLElements
{
    public class CreateModel : BLElementCategoriesPageModel
	{
        private readonly BucketListApplication.Data.BLContext _context;

		[BindProperty]
		public BucketListElement BucketListElement { get; set; }

		public CreateModel(BucketListApplication.Data.BLContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(int bucketlistid)
        {
			if (!User.Identity.IsAuthenticated)
				return RedirectToPage("../AuthError");

			BucketListElement = new BucketListElement
			{
				BucketListID = bucketlistid,
				BucketList = _context.BucketLists.Find(bucketlistid),
				ElementCategories = new List<ElementCategory>()
			};

			PopulateAssignedCategoryData(_context, BucketListElement);
			PopulateBucketListDropDownList(_context);
			return Page();
		}

        public async Task<IActionResult> OnPostAsync(int bucketlistid, string[] selectedCategories)
        {
			if (!User.Identity.IsAuthenticated)
				return RedirectToPage("../AuthError");

			var newBLElement = new BucketListElement
			{
				BucketListID = bucketlistid,
				Progression = new Progression
				{
					BLETasks = new List<BLETask>()
				}
			};

			if (selectedCategories != null)
			{
				newBLElement.ElementCategories = new List<ElementCategory>();
				foreach (var category in selectedCategories)
				{
					var categoryToAdd = new ElementCategory
					{
						CategoryID = int.Parse(category)
					};
					newBLElement.ElementCategories.Add(categoryToAdd);
				}
			}

			// Defense against overposting attacks. Returns true if the update was successful.
			if (await TryUpdateModelAsync<BucketListElement>(newBLElement, "BucketListElement",
				ble => ble.Name,
				ble => ble.Description,
				ble => ble.Completed,
				ble => ble.Visibility,
				ble => ble.Progression))
			{
				_context.BLElements.Add(newBLElement);
				await _context.SaveChangesAsync();
				return RedirectToPage("../BucketLists/Index", new { selectedbucketlistid = bucketlistid });
			}
			//If TryUpdateModelAsync fails restore AssignedCategoryDataList and DropDownLists
			PopulateAssignedCategoryData(_context, newBLElement);
			PopulateBucketListDropDownList(_context, newBLElement.BucketListID);
			return Page();
		}
    }
}
