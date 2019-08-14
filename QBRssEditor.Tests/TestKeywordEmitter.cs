using Microsoft.VisualStudio.TestTools.UnitTesting;
using QBRssEditor.Services.KeywordEmitter;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace QBRssEditor.Tests
{
    [TestClass]
    public class TestKeywordEmitter
    {
        private static IEnumerable<IKeywordEmitter> GetEmitters() =>
            ServicesBuilder.CreateServiceProvider().GetRequiredService<IEnumerable<IKeywordEmitter>>();

        private static List<string> GetKeywords(string title) =>
            GetEmitters().SelectMany(z => z.GetKeywords(title)).ToList();

        [TestMethod]
        public void TestSeriesKeywordEmitter()
        {
            CollectionAssert.Contains(
                GetKeywords("推理剧场.Mystery.Theater.E62.中文字幕.1280×720.HDTVrip-拉风字幕组 .mp4【ciLi001.com】"),
                "推理剧场.Mystery.Theater");
        }

        [TestMethod]
        public void TestFirstElementKeywordEmitter()
        {
            var emitter = new FirstElementKeywordEmitter();
            var keywords = emitter
                .GetKeywords("【咪梦动漫组】★4月新番★【超可动女孩1/6 / 超可動ガール1/6 / Chou Kadou Girl 1/6：Amazing Stranger】【05】【1080P】【简繁外挂字幕】")
                .ToList();
            CollectionAssert.AreEqual(new string[] { }, keywords);
        }

        [TestMethod]
        public void TestPartsKeywordEmitter()
        {
            var emitter = new PartsKeywordEmitter();
            var keywords = emitter
                .GetKeywords("【咪梦动漫组】★4月新番★【超可动女孩1/6 / 超可動ガール1/6 / Chou Kadou Girl 1/6：Amazing Stranger】【05】【1080P】【简繁外挂字幕】")
                .ToList();
            CollectionAssert.AreEqual(
                new [] {
                    "咪梦动漫组",
                    "超可动女孩1/6 / 超可動ガール1/6 / Chou Kadou Girl 1/6：Amazing Stranger",
                    "05",
                    "1080P",
                    "简繁外挂字幕"
                }, 
                keywords);
        }
    }
}
