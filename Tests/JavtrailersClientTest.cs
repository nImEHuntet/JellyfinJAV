namespace Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using JellyfinJav.Api;
    using NUnit.Framework;

#pragma warning disable SA1606
#pragma warning disable SA1600 // Elements should be documented
    public class JavtrailersClientTest
#pragma warning restore SA1600 // Elements should be documented
    {
        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task TestSearchMany()
        {
            var results = await JavtrailersClient.Search("INSTV").ConfigureAwait(false);

            Assert.AreEqual(24, results.Count());
            Assert.AreEqual("INSTV-551", results.ElementAt(5).Code);
            Assert.AreEqual("h_1472instv00551", results.ElementAt(5).Id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task TestSearchSingle()
        {
            var results = await JavtrailersClient.Search("SDDE-592").ConfigureAwait(false);

            Assert.AreEqual("SDDE-592", results.ElementAt(0).Code);
            Assert.AreEqual("1sdde00592", results.ElementAt(0).Id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task TestSearchFirstNoResults()
        {
            var result = await JavtrailersClient.SearchFirst("6942069").ConfigureAwait(false); // Javtrailers makes it really hard to have 0 results

            Assert.AreEqual(null, result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task TestSearchFirstInvalid()
        {
            var result = await JavtrailersClient.SearchFirst("there shouldn't be any").ConfigureAwait(false); // Javtrailers makes it really hard to have any invalids

            Assert.AreEqual(null, result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task TestSearchFirstSingleResult()
        {
            var result = await JavtrailersClient.SearchFirst("SSNI-230").ConfigureAwait(false);

            var correct = new Video(
                id: "javli7bvzi",
                code: "SSNI-230",
                title: "Big Slap Brilliantly Seductive Ass Pub Miss",
                actresses: new[] { "Hoshino Nami" },
                genres: new[] { "Solowork", "Nasty, Hardcore", "Cowgirl", "Prostitutes", "Butt", "Risky Mosaic", "Huge Butt" },
                studio: "S1 NO.1 STYLE",
                boxArt: "https://pics.dmm.co.jp/mono/movie/adult/ssni230/ssni230pl.jpg",
                cover: "https://pics.dmm.co.jp/mono/movie/adult/ssni230/ssni230ps.jpg",
                releaseDate: null); // TODO

            Assert.AreEqual(correct, result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task TestLoadVideoNormalizeTitle()
        {
            var result = await JavtrailersClient.LoadVideo("SDDE-592").ConfigureAwait(false);

            var correct = new Video(
                id: "javli6lg24",
                code: "STARS-126",
                title: "A Big Ass Pantyhose Woman Who Is Exposed So Much That There Is No Plump",
                actresses: new[] { "Koizumi Hinata" },
                genres: new[] { "Cosplay", "Solowork", "Beautiful Girl", "Huge Butt" },
                studio: "SOD Create",
                boxArt: "https://pics.dmm.co.jp/mono/movie/adult/1stars126/1stars126pl.jpg",
                cover: "https://pics.dmm.co.jp/mono/movie/adult/1stars126/1stars126ps.jpg",
                releaseDate: null); // TODO

            Assert.AreEqual(correct, result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task TestLoadVideoOneActress()
        {
            var result = await JavtrailersClient.LoadVideo("ABP-002").ConfigureAwait(false);

            var correct = new Video(
                id: "javlio354u",
                code: "ABP-002",
                title: "NEW TOKYO Style 01 Aika Phosphorus",
                actresses: new[] { "Aikarin" },
                genres: new[] { "Handjob", "Solowork", "Facials" },
                studio: "Prestige",
                boxArt: "https://pics.dmm.co.jp/mono/movie/adult/118abp002/118abp002pl.jpg",
                cover: "https://pics.dmm.co.jp/mono/movie/adult/118abp002/118abp002ps.jpg",
                releaseDate: null); // TODO

            Assert.AreEqual(correct, result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task TestLoadVideoManyActresses()
        {
            var result = await JavtrailersClient.LoadVideo("SDDE-592").ConfigureAwait(false);

            var correct = new Video(
                id: "javli6bm5q",
                code: "SDDE-592",
                title: "Room Boundaries-If It Were In This Way, I Would Like It!To",
                actresses: new[] { "Kurata Mao", "Mihara Honoka", "Kururigi Aoi" },
                genres: new[] { "Cosplay", "Planning", "Cum", "Hypnosis" },
                studio: "SOD Create",
                boxArt: "https://pics.dmm.co.jp/mono/movie/adult/1sdde592/1sdde592pl.jpg",
                cover: "https://pics.dmm.co.jp/mono/movie/adult/1sdde592/1sdde592ps.jpg",
                releaseDate: null); // TODO

            Assert.AreEqual(correct, result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task TestLoadVideoNoActresses()
        {
            var result = await JavtrailersClient.LoadVideo("IPTD-041").ConfigureAwait(false);

            var correct = new Video(
                id: "javliarg3u",
                code: "IPTD-041",
                title: "Goddesses Of The Speed Of Sound 01 RQ'S Cafe",
                actresses: Array.Empty<string>(),
                genres: new[] { "Mini Skirt", "Big Tits", "Slender", "Race Queen", "Digital Mosaic" },
                studio: "IDEA POCKET",
                boxArt: "https://pics.dmm.co.jp/mono/movie/adult/iptd041/iptd041pl.jpg",
                cover: "https://pics.dmm.co.jp/mono/movie/adult/iptd041/iptd041ps.jpg",
                releaseDate: null); // TODO

            Assert.AreEqual(correct, result);
        }
    }
}
#pragma warning restore SA1606