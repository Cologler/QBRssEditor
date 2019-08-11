using Microsoft.VisualStudio.TestTools.UnitTesting;
using QBRssEditor.Services.KeywordEmitter;
using System.Linq;

namespace QBRssEditor.Tests
{
    [TestClass]
    public class TestKeywordEmitter
    {
        [TestMethod]
        public void TestSeriesKeywordEmitter()
        {
            var emitter = new SeriesKeywordEmitter();
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
