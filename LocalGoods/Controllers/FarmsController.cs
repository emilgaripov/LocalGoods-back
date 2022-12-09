using LocalGoods.BAL.DTOs;
using LocalGoods.BAL.Services.Implementation;
using LocalGoods.BAL.Services.Interfaces;
using LocalGoods.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using NuGet.Protocol;

namespace LocalGoods.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class FarmsController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IFarmService farmService;
        private readonly IProductService productService;
        private readonly IWebHostEnvironment webHostEnvironment;

        public FarmsController(IFarmService farmService,IProductService productService, UserManager<User> userManager,IWebHostEnvironment webHostEnvironment)
        {
            this.farmService = farmService;
            this.productService = productService;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        [Authorize]
        [HttpPost("{UserId}")]
        public async Task<ActionResult<ViewFarmDTO>> Create(string UserId,[FromForm]CreateFarmDTO farmDTO)
        {
            if(ModelState.IsValid)
            {
                string uniqueFileName=ProcessUploadedFile(farmDTO);
                farmDTO.UserId = UserId;
                (FarmDTO createdFarm, int i) = await farmService.Create(farmDTO,uniqueFileName);
                if (i == 0)
                {
                    return NotFound("User Not Found");
                }
                else if (i == 1)
                {
                    return Ok(createdFarm);
                }
                else if (i == 2)
                {
                    return StatusCode(501);
                }
                else
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }
        [HttpGet]
        public async Task<ActionResult<List<ViewFarmDTO>>> Get()
        {
            return Ok(await farmService.GetAll());
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<FarmDTO>> GetById(int id)
        {
            FarmDTO? farm = await farmService.Get((int)id);
            if(farm is null)
            {
                return NotFound();
            }
            return Ok(farm);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<int>> Delete(int id)
        {
            int i = await farmService.Delete((int)id);
            if(i==1)
            {
                return Ok("Deleted Successfully");
            }
            else if(i==0)
            {
                return NotFound();
            }
            else if(i==2)
            {
                return StatusCode(501);
            }
            return BadRequest();
         }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, FarmDTO farmDTO)
        {
            farmDTO.Id = id;
            (farmDTO, int i) = await farmService.Update(farmDTO);
            if (i == 1)
            {
                return Ok(farmDTO);
            }
            else if (i == 0)
            {
                return NotFound("Farm Not Found");
            }
            else if (i == 2)
            {
                return StatusCode(501);
            }
            return BadRequest();
        }
        [HttpGet("{id}/FarmProducts")]
        public async Task<ActionResult> FarmProducts(int id)
        {
            (List<ProductDTO> products, int i) = await farmService.GetProducts(id);
            if(i==0)
            {
                return NotFound("Farm Not Found");
            }
            else if(i==1)
            {
                return Ok(products);
            }
            return Ok(products);
        }
        private string ProcessUploadedFile(CreateFarmDTO model)
        {
            string uniqueFileName = "";

            if (model.ImageFile != null)
            {
                string uploadsFolder = webHostEnvironment.WebRootPath;
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder + "/Images/Farms/" + uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImageFile.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
