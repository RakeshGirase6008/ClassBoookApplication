using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Topics;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
        private readonly ClassBookModelFactory _classBookModelFactory;


        #endregion

        #region Ctor

        public TopicController(ClassBookManagementContext context,
            FileService fileService,
            ClassBookModelFactory classBookModelFactory)
        {
            this._context = context;
            this._fileService = fileService;
            this._classBookModelFactory = classBookModelFactory;
        }

        #endregion

        // POST api/Topic/AddEditTopic
        [HttpPost("AddEditTopic")]
        public IActionResult AddEditTopic([FromForm] TopicCommonRequestModel model)
        {
            ResponseModel responseModel = new ResponseModel();

            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    // Adding New Topic
                    Topic topic = new Topic();
                    topic.OrderItemId = model.OrderItemId;
                    topic.Name = model.Name;
                    topic.Description = model.Description;
                    if (model.Files != null && model.Files.Count > 0)
                        topic.ImageUrl = _fileService.SaveFile(model.Files, ClassBookConstant.ImagePath_Topic);
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
                    if (model.Files.Count > 0)
                        topic.ImageUrl = _fileService.SaveFile(model.Files, ClassBookConstant.ImagePath_Topic);
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


        // POST api/Topic/AddEditSubTopic
        [HttpPost("AddEditSubTopic")]
        public IActionResult AddEditSubTopic([FromForm] TopicCommonRequestModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    SubTopic subTopic = new SubTopic();
                    subTopic.TopicId = model.TopicId;
                    subTopic.Name = model.Name;
                    subTopic.Description = model.Description;
                    subTopic.ImageUrl = _fileService.SaveFile(model.Files, ClassBookConstant.ImagePath_Topic);
                    subTopic.VideoLink = _fileService.SaveFile(model.Video, ClassBookConstant.VideoPath_Topic);
                    subTopic.DateOfUpload = DateTime.Now;
                    subTopic.DateOfActivation = model.DateOfActivation;
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
                    subTopic.DateOfActivation = model.DateOfActivation;
                    if (model.Files.Count > 0)
                        subTopic.ImageUrl = _fileService.SaveFile(model.Files, ClassBookConstant.ImagePath_Topic);
                    if (model.Video.Count > 0)
                        subTopic.VideoLink = _fileService.SaveFile(model.Video, ClassBookConstant.VideoPath_Topic);
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


        // GET api/Topic/GetTopicData/6
        [HttpGet("GetTopicData/{id:int}")]
        public IEnumerable<object> GetTopicData(int id)
        {
            List<TopicResponseModel> allTopicList = new List<TopicResponseModel>();
            var allTopics = _context.Topic.Where(x => x.OrderItemId == id).ToList();
            if (allTopics != null)
            {
                foreach (var topic in allTopics)
                {
                    TopicResponseModel topicResponseModel = new TopicResponseModel();
                    topicResponseModel.Name = topic.Name;
                    topicResponseModel.Description = topic.Description;
                    topicResponseModel.ImageUrl = _classBookModelFactory.PrepareURL(topic.ImageUrl);


                    var allSubjectTopics = _context.SubTopic.Where(x => x.TopicId == topic.Id).ToList();
                    if (allSubjectTopics != null)
                    {
                        topicResponseModel.SubTopicCount = allSubjectTopics.Count;
                        List<SubTopicResponseModel> allSubTopci = new List<SubTopicResponseModel>();
                        foreach (var subTopic in allSubjectTopics)
                        {
                            SubTopicResponseModel subTopicResponseModel = new SubTopicResponseModel();
                            subTopicResponseModel.Name = subTopic.Name;
                            subTopicResponseModel.Description = subTopic.Description;
                            subTopicResponseModel.ImageUrl = _classBookModelFactory.PrepareURL(subTopic.ImageUrl);
                            subTopicResponseModel.VideoLink = _classBookModelFactory.PrepareURL(subTopic.VideoLink);

                            allSubTopci.Add(subTopicResponseModel);
                        }
                        topicResponseModel.subTopicResponseModel = allSubTopci;
                    }
                    allTopicList.Add(topicResponseModel);
                }
            }
            return allTopicList;
        }
    }
}
