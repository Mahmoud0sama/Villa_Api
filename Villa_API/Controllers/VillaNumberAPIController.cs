﻿using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Controllers
{
	[Route("api/v{version:apiVersion}/VillaNumberAPI")]
	[ApiController]
	[ApiVersion("1.0")]
	[ApiVersion("2.0")]
	public class VillaNumberAPIController : ControllerBase
	{
		protected APIResponse _response;
		private readonly IVillaNumberRepository _dbVillaNumber;
		private readonly IVillaRepository _dbVilla;
		private readonly IMapper _mapper;
		public VillaNumberAPIController(IVillaNumberRepository dbVillaNumber,IVillaRepository dbVilla, IMapper mapper)
		{
			_dbVillaNumber = dbVillaNumber;
			_dbVilla = dbVilla;
			_mapper = mapper;
			this._response = new();
		}
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<APIResponse>> GetVillaNumbers()
		{
			try
			{
				IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync(includeProperties:"Villa");
				_response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
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
		[HttpGet("{id:int}", Name = "GetVillaNumber")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
		{
			try
			{
				if (id == 0)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccessfull = false;
					return BadRequest(_response);
				}
				var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
				if (villaNumber == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccessfull = false;
					return NotFound(_response);
				}
				_response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
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
		[Authorize(Roles ="admin")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
		{
			try
			{
				if (await _dbVillaNumber.GetAsync(u => u.VillaNo == createDTO.VillaNo) != null)
				{
					ModelState.AddModelError("ErrorMessages", "Villa Number Already Exists!");
					_response.IsSuccessfull = false;
					return BadRequest(ModelState);
				}
				if(await _dbVilla.GetAsync(u=>u.Id == createDTO.VillaId) == null)
				{
					ModelState.AddModelError("ErrorMessages", "Villa Id Is Invalid!");
					_response.IsSuccessfull = false;
					return BadRequest(ModelState);
				}

				
				if (createDTO == null)
				{
					return BadRequest(createDTO);
				}
				VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDTO);
				await _dbVillaNumber.CreateAsync(villaNumber);
				_response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
				_response.StatusCode = HttpStatusCode.Created;
				return CreatedAtRoute("GetVillaNumber", new { id = villaNumber.VillaNo }, _response);
			}
			catch (Exception ex)
			{
				_response.IsSuccessfull = false;
				_response.ErrorMessages = new List<string> { ex.ToString() };
			}
			return _response;
		}
		[Authorize(Roles = "admin")]
		[HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
		{
			try
			{
				if (id == 0)
				{
					return BadRequest();
				}
				var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
				if (villaNumber == null)
				{
					return NotFound();
				}
				await _dbVillaNumber.RemoveAsync(villaNumber);
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
		[Authorize(Roles = "admin")]
		[HttpPut("{id:int}", Name = "UpdateVillaNumber")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id,[FromBody] VillaNumberUpdateDTO updateDTO)
		{
			try
			{
				if (updateDTO == null || id != updateDTO.VillaNo)
				{
					_response.IsSuccessfull = false;
					return BadRequest();
				}
				if (await _dbVilla.GetAsync(u => u.Id == updateDTO.VillaId) == null)
				{
					ModelState.AddModelError("ErrorMessages", "Villa Id Is Invalid!");
					return BadRequest(ModelState);
				}
				VillaNumber villaNumber = _mapper.Map<VillaNumber>(updateDTO);
				await _dbVillaNumber.UpdateAsync(villaNumber);
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
	}
}
