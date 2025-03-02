using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Villa_Utility;
using Villa_Web.Models;
using Villa_Web.Models.Dto;
using Villa_Web.Services.IServices;

namespace Villa_Web.Controllers
{
	public class VillaController : Controller
	{
		private readonly IVillaService _villaService;
		private readonly IMapper _mapper;
		public VillaController(IVillaService villaService, IMapper mapper)
		{
			_villaService = villaService;
			_mapper = mapper;
		}
		public async Task<IActionResult> IndexVilla()
		{
			List<VillaDTO> list = new();
			var response = await _villaService.GetAllAsync<APIResponse>(HttpContext.Session.GetString(SD.SessionToken));
			if (response != null && response.IsSuccessfull)
			{
				list = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(response.Result));
			}
			return View(list);
		}
		[Authorize(Roles ="admin")]
		public async Task<IActionResult> CreateVilla()
		{
			return View();
		}
		[HttpPost]
		[Authorize(Roles = "admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateVilla(VillaCreateDTO model)
		{
			if (ModelState.IsValid)
			{
				var response = await _villaService.CreateAsync<APIResponse>(model, HttpContext.Session.GetString(SD.SessionToken));
				if (response != null && response.IsSuccessfull)
				{
					TempData["success"] = "Villa Created Successfully";
					return RedirectToAction(nameof(IndexVilla));
				}
			}
            TempData["error"] = "Error Encountered.";
            return View(model);
		}
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> UpdateVilla(int villaId)
		{
			var response = await _villaService.GetAsync<APIResponse>(villaId, HttpContext.Session.GetString(SD.SessionToken));
			if (response != null && response.IsSuccessfull)
			{
				VillaDTO model = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
				return View(_mapper.Map<VillaUpdateDTO>(model));
			}
			return NotFound();
		}
		[HttpPost]
		[Authorize(Roles = "admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateVilla(VillaUpdateDTO model)
		{
			if (ModelState.IsValid)
			{

				var response = await _villaService.UpdateAsync<APIResponse>(model, HttpContext.Session.GetString(SD.SessionToken));
				if (response != null && response.IsSuccessfull)
				{
                    TempData["success"] = "Villa Updated Successfully";
                    return RedirectToAction(nameof(IndexVilla));
				}
			}
            TempData["error"] = "Error Encountered.";
            return View(model);
		}
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> DeleteVilla(int villaId)
		{
			var response = await _villaService.GetAsync<APIResponse>(villaId, HttpContext.Session.GetString(SD.SessionToken));
			if (response != null && response.IsSuccessfull)
			{
				VillaDTO model = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
				return View(model);
			}
			return NotFound();
		}
		[HttpPost]
		[Authorize(Roles = "admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteVilla(VillaDTO model)
		{
			var response = await _villaService.DeleteAsync<APIResponse>(model.Id, HttpContext.Session.GetString(SD.SessionToken));
			if (response != null && response.IsSuccessfull)
			{
                TempData["success"] = "Villa Deleted Successfully";
                return RedirectToAction(nameof(IndexVilla));
			}
            TempData["error"] = "Error Encountered.";
            return View(model);
		}
	}
}
