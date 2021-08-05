using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using System.Threading;
using Newtonsoft.Json;
using Serilog;

namespace ConsolePubSubApp.Models
{
    public class Subscriber
    {
        public static IConfiguration Configuration {get; private set;}

        public Subscriber(IConfiguration _config)
        {
            Configuration = _config;
        }

        public async Task Subscription()
        {
            try
            {
                var project_id = Configuration.GetSection("ProjectId").Value;
                var subscription_id = Configuration.GetSection("SubScriptionId").Value;
                var topic_id = Configuration.GetSection("TopicId").Value;

                SubscriptionName subscriptionName = new SubscriptionName(project_id,subscription_id);

                SubscriberServiceApiClient subscriberClient = SubscriberServiceApiClient.Create();
                
                //Create SubScription
                try
                {
                    subscriberClient.CreateSubscription(subscription_id,topic_id,pushConfig:null,ackDeadlineSeconds:300);
                }catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists || e.Status.StatusCode == StatusCode.NotFound)
                {
                    Log.Information($"Subscription status {e.Status.StatusCode} {e.Status.Detail}");
                }
                catch (Exception e)
                {
                    Log.Information($"Exeption while creating subscription of topic {subscription_id}");
                }

                SubscriberClient objSubscriberClient= await SubscriberClient.CreateAsync(subscriptionName);
                if (objSubscriberClient != null)
                {
                    Data data;
                    try
                    {
                        await objSubscriberClient.StartAsync((message, cancellationToken) =>
                        {
                            data = JsonConvert.DeserializeObject<Data>(message.Data.ToStringUtf8());
                            return Task.FromResult(SubscriberClient.Reply.Ack);
                        });
                    }
                    catch (Exception ex)
                    {
                        await objSubscriberClient.StopAsync(CancellationToken.None);
                    }
                }
                else
                {
                    Log.Information($"Subscriber Client {objSubscriberClient}");
                }
            }
            catch (Exception ex)
            {
                Log.Information($"Exception {ex.ToString()}");
            }
        }
    }
}
