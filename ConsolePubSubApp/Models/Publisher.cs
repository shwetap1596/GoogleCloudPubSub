using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsolePubSubApp.Models
{
    public class Publisher
    {
        public static IConfiguration Configuration { get; private set; }

        public Publisher(IConfiguration _config)
        {
            Configuration = _config;
        }

        public async Task Publish()
        {
            try
            {
                var project_id = Configuration.GetSection("ProjectId").Value;
                var topic_id = Configuration.GetSection("TopicId").Value;

                TopicName topicName = new TopicName(project_id, topic_id);

                PublisherServiceApiClient publisherClient = PublisherServiceApiClient.Create();

                //Create topic
                try
                {
                    publisherClient.CreateTopic(topic_id);
                }
                catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists || e.Status.StatusCode == StatusCode.NotFound)
                {
                    Log.Information($"Publisher status {e.Status.StatusCode} {e.Status.Detail}");
                }
                catch (Exception e)
                {
                    Log.Information($"Exeption while creating subscription of topic {topic_id}");
                }

                PublisherClient objPublishClient = await PublisherClient.CreateAsync(topicName);
                if(objPublishClient != null)
                {
                    string publishMessage = JsonConvert.SerializeObject(new Data() { messageId = "1" });
                    await objPublishClient.PublishAsync(publishMessage);
                    await objPublishClient.ShutdownAsync(TimeSpan.FromSeconds(60));
                }
                else
                {
                    Log.Information($"PublishClient : {objPublishClient}");
                }
            }
            catch(Exception ex)
            {
                Log.Information($"Exception: {ex.ToString()}");
            }
        }
    }
}
