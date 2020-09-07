using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Topics;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;

namespace ClassBookApplication.Controllers.API
{
    [ApiVersion("1")]
    public class TopicController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly FileService _fileService;


        #endregion

        #region Ctor

        public TopicController(ClassBookManagementContext context,
            FileService fileService)
        {
            this._context = context;
            this._fileService = fileService;
        }

        #endregion

        // POST api/Topic/AddTopic
        [HttpPost("AddTopic")]
        public IActionResult AddEditTopic([FromForm] TopicCommonRequestModel model)
        {
            ResponseModel responseModel = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    Topic topic = new Topic();
                    topic.Name = model.Name;
                    topic.Description = model.Description;
                    topic.ImageUrl = _fileService.SaveFile(model.files, ClassBookConstant.ImagePath_Topic);
                    topic.Deleted = false;
                    topic.Active = true;
                    _context.Topic.Add(topic);
                    _context.SaveChanges();
                    return StatusCode((int)HttpStatusCode.OK);
                }
                else
                {
                    var topic = _context.Topic.Where(x => x.Id == model.Id).FirstOrDefault();
                    topic.Name = model.Name;
                    topic.Description = model.Description;
                    if (model.files.Count > 0)
                    {
                        topic.ImageUrl = _fileService.SaveFile(model.files, ClassBookConstant.ImagePath_Topic);
                    }
                    _context.Topic.Update(topic);
                    _context.SaveChanges();
                    return StatusCode((int)HttpStatusCode.OK);
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }
        }


        // POST api/Topic/AddSubTopic
        [HttpPost("AddSubTopic")]
        public IActionResult AddSubTopic([FromForm] TopicCommonRequestModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    SubTopic subTopic = new SubTopic();
                    subTopic.Name = model.Name;
                    subTopic.Description = model.Description;
                    subTopic.ImageUrl = _fileService.SaveFile(model.files, ClassBookConstant.ImagePath_Topic);
                    subTopic.VideoLink = model.VideoLink;
                    subTopic.DateOfUpload = DateTime.Now;
                    subTopic.Deleted = false;
                    subTopic.Active = true;
                    _context.SubTopic.Add(subTopic);
                    _context.SaveChanges();
                    return StatusCode((int)HttpStatusCode.OK);
                }
                else
                {
                    var subTopic = _context.SubTopic.Where(x => x.Id == model.Id).FirstOrDefault();
                    subTopic.Name = model.Name;
                    subTopic.Description = model.Description;
                    subTopic.VideoLink = model.VideoLink;
                    if (model.files.Count > 0)
                    {
                        subTopic.ImageUrl = _fileService.SaveFile(model.files, ClassBookConstant.ImagePath_Topic);
                    }
                    _context.SubTopic.Update(subTopic);
                    _context.SaveChanges();
                    return StatusCode((int)HttpStatusCode.OK);
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }
        }
    }
}
