using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolf.Notification.Database.Entities;
using Wolf.Notification.Models;

namespace Wolf.Notification.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class ProviderController : ControllerBase
	{
		private readonly NotifDbContext _dbContext;
		IMapper _mapper;

		public ProviderController(NotifDbContext dbContext, IMapper mapper)
		{
			_dbContext = dbContext;
			_mapper = mapper;
		}

		[HttpGet("")]
		public async Task<IEnumerable<ProviderDto>> Get()
		{
			var providers= await  _dbContext.Providers.ToListAsync();
			return _mapper.Map<IEnumerable<ProviderDto>>(providers);
		}
	}
}
