using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Controllers
{
	[Route("api/v{version:apiVersion}/VillaAPI")]
	[ApiController]
	[ApiVersion("1.0")]
	public class VillaAPIController : ControllerBase
	{
		protected APIResponse _response;
		private readonly IVillaRepository _dbVilla;
		private readonly IMapper _mapper;
		public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
		{
			_dbVilla = dbVilla;
			_mapper = mapper;
			this._response = new();
		}
		[HttpGet]
		[ResponseCache(CacheProfileName = "Default30")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<ActionResult<APIResponse>> GetVillas([FromQuery] int? occupancy,
			[FromQuery] string? search, int pageSize = 0, int pageNumber = 1)
		{
			try
			{
				IEnumerable<Villa> villaList;
				if (occupancy > 0)
				{
					villaList = await _dbVilla.GetAllAsync(u => u.Occupancy == occupancy, pageSize: pageSize,
						pageNumber: pageNumber);
				}
				else
				{
					villaList = await _dbVilla.GetAllAsync(pageSize: pageSize,
						pageNumber: pageNumber);
				}
				if (!string.IsNullOrEmpty(search))
				{
					villaList = villaList.Where(u => u.Name.ToLower().Contains(search));
				}
				Pagination pagination = new() { PageNumber = pageNumber, PageSize = pageSize };

				Response.Headers.Add("X-Pagination",JsonSerializer.Serialize(pagination));
				_response.Result = _mapper.Map<List<VillaDTO>>(villaList);
				_response.StatusCode = HttpStatusCode.OK;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.IsSuccessfull = false;
				_response.ErrorMessages = new List<string> { ex.ToString() };
			}
			return _response;
		}
		[HttpGet("{id:int}", Name = "GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<APIResponse>> GetVilla(int id)
		{
			try
			{
				if (id == 0)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccessfull = false;
					return BadRequest(_response);
				}
				var villa = await _dbVilla.GetAsync(u => u.Id == id);
				if (villa == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccessfull = false;
					return NotFound(_response);
				}
				_response.Result = _mapper.Map<VillaDTO>(villa);
				_response.StatusCode = HttpStatusCode.OK;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.IsSuccessfull = false;
				_response.ErrorMessages = new List<string> { ex.ToString() };
			}
			return _response;
		}
		[HttpPost]
		[Authorize(Roles = "admin")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
		{
			try
			{
				if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
				{
					ModelState.AddModelError("ErrorMessages", "Villa Already Exists!");
					_response.IsSuccessfull = false;
					return BadRequest(ModelState);
				}
				if (createDTO == null)
				{
					_response.IsSuccessfull = false;
					return BadRequest(createDTO);
				}
				Villa villa = _mapper.Map<Villa>(createDTO);
				await _dbVilla.CreateAsync(villa);
				_response.Result = _mapper.Map<VillaDTO>(villa);
				_response.StatusCode = HttpStatusCode.Created;
				return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
			}
			catch (Exception ex)
			{
				_response.IsSuccessfull = false;
				_response.ErrorMessages = new List<string> { ex.ToString() };
			}
			return _response;
		}
		[HttpDelete("{id:int}", Name = "DeleteVilla")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
		{
			try
			{
				if (id == 0)
				{
					return BadRequest();
				}
				var villa = await _dbVilla.GetAsync(u => u.Id == id);
				if (villa == null)
				{
					return NotFound();
				}
				await _dbVilla.RemoveAsync(villa);
				_response.StatusCode = HttpStatusCode.NoContent;
				_response.IsSuccessfull = true;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.IsSuccessfull = false;
				_response.ErrorMessages = new List<string> { ex.ToString() };
			}
			return _response;
		}
		[HttpPut("{id:int}", Name = "UpdateVilla")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
		{
			try
			{
				if (updateDTO == null || id != updateDTO.Id)
				{
					_response.IsSuccessfull = false;
					return BadRequest();
				}
				Villa villa = _mapper.Map<Villa>(updateDTO);
				await _dbVilla.UpdateAsync(villa);
				_response.StatusCode = HttpStatusCode.NoContent;
				_response.IsSuccessfull = true;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.IsSuccessfull = false;
				_response.ErrorMessages = new List<string> { ex.ToString() };
			}
			return _response;
		}
		[HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
		{
			if (patchDTO == null || id == 0)
			{
				return BadRequest();
			}
			var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);
			VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);
			if (villa == null)
			{
				return BadRequest();
			}
			patchDTO.ApplyTo(villaDTO, ModelState);
			Villa model = _mapper.Map<Villa>(villaDTO);
			await _dbVilla.UpdateAsync(model);
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			return NoContent();
		}
	}
}
