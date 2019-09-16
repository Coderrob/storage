﻿using Xunit;
using Storage.Net.Messaging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Storage.Net.Microsoft.Azure.ServiceBus;

namespace Storage.Net.Tests.Integration.Messaging
{
   [Trait("Category", "Messenger")]
   public abstract class MessagingTest : IAsyncLifetime
   {
      private readonly MessagingFixture _fixture;
      private readonly string _channelPrefix;
      private readonly IMessenger _msg;
      private readonly string _qn;

      protected MessagingTest(MessagingFixture fixture, string channelPrefix = null)
      {
         _fixture = fixture;
         _channelPrefix = channelPrefix;
         _qn = NewChannelName();
         _msg = fixture.Messenger;
      }

      public async Task InitializeAsync()
      {
         await _msg.CreateChannelAsync(_qn);
      }

      private string NewChannelName()
      {
         return $"{_channelPrefix}{Guid.NewGuid().ToString()}";
      }

      public async Task DisposeAsync()
      {
         //clear up all the channels

         try
         {
            IReadOnlyCollection<string> channels = await _msg.ListChannelsAsync();
            await _msg.DeleteChannelsAsync(channels);
         }
         catch { }
      }

      [Fact]
      public async Task SendMessage_OneMessage_DoesntCrash()
      {
         var qm = QueueMessage.FromText("test");

         await _msg.SendAsync(_qn, qm);
      }

      [Fact]
      public async Task SendMessage_NullChannel_ArgumentException()
      {
         await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.SendAsync(null, QueueMessage.FromText("test")));
      }

      [Fact]
      public async Task SendMessage_NullMessages_ArgumentException()
      {
         await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.SendAsync(_qn, null));
      }

      [Fact]
      public async Task Channels_list_doesnt_crash()
      {
         await _msg.ListChannelsAsync();
      }

      [Fact]
      public async Task Channels_Create_list_contains_created_channel()
      {
         string channelName = NewChannelName();

         //send one message so channel gets created
         await _msg.CreateChannelAsync(channelName);

         IReadOnlyCollection<string> channels = await _msg.ListChannelsAsync();

         Assert.Contains(channelName, channels);
      }

      [Fact]
      public async Task Channels_delete_goesaway()
      {
         string channelName = NewChannelName();

         await _msg.CreateChannelAsync(channelName);

         await _msg.DeleteChannelAsync(channelName);

         IReadOnlyCollection<string> channels = await _msg.ListChannelsAsync();

         Assert.DoesNotContain(channelName, channels);

      }

      [Fact]
      public async Task Channels_delete_null_list_argument_exception()
      {
         await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.DeleteChannelsAsync(null));
      }

   }
}