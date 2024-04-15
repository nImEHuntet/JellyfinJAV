#pragma warning disable SA1600

namespace Tests
{
    using JellyfinJav.Api;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class R18ClientTest
    {
        [Test]
        public async Task TestSearchMany()
        {
            var results = await R18Client.Search("sora").ConfigureAwait(false);

            Assert.AreEqual(null, results);
        }

        [Test]
        public async Task TestSearchNone()
        {
            var results = await R18Client.Search("noresult").ConfigureAwait(false);

            Assert.AreEqual(0, results.Count());
        }

        [Test]
        public async Task TestSearchFirst()
        {
            var expected = new Video(
                id: "mudr25",
                code: "MUDR-255",
                title: "Female Shinobi Training: Shinobi Fallen - Live Action Version",
                actresses: new[] { "Waka Misono" },
                genres: new[] { "Big Tits", "BDSM", "Featured Actress", "Original Collaboration", "Bondage", "Sample Video" },
                studio: "Muku",
                boxArt: "https://pics.dmm.co.jp/mono/movie/adult/mudr255/mudr255ps.jpg",
                cover: "https://pics.dmm.co.jp/mono/movie/adult/mudr255/mudr255pl.jpg",
                releaseDate: DateTime.Parse("2024-04-16"));

            var result = await R18Client.SearchFirst("mudr-255").ConfigureAwait(false);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task TestSearchFirstNone()
        {
            var result = await R18Client.SearchFirst("testdata").ConfigureAwait(false);

            Assert.AreEqual(null, result);
        }

        [Test]
        public async Task TestLoadVideo()
        {
            var expected = new Video(
                id: "ssni00643",
                code: "SSNI-643",
                title: "A Woman With Divine Titties Lala Anzai Her Adult Video Debut Miraculous Huge Tits A 7-Tit-Jamming Special",
                actresses: new[] { "Rara Anzai" },
                genres: new[] { "Big Tits", "Big Tits Lover", "Featured Actress", "Titty Fuck", "Debut", "Minimal Mosaic", "Hi-Def", "Exclusive Distribution" },
                studio: "S1 NO.1 STYLE",
                boxArt: "https://awsimgsrc.dmm.com/dig/digital/video/ssni00643/ssni00643ps.jpg",
                cover: "https://awsimgsrc.dmm.com/dig/digital/video/ssni00643/ssni00643pl.jpg",
                releaseDate: DateTime.Parse("7/12/2019"));

            var result = await R18Client.LoadVideo("ssni00643").ConfigureAwait(false);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task TestLoadVideoNoActress()
        {
            var expected = new Video(
                id: "njvr00023",
                code: "NJVR-023",
                title: "[VR] The Horn Dogs Chose My Apartment To Be Their Fuck Pad. My Friend Was A Nampa Artist And He Brought Over Tsubasa-chan For Some Lotion Lathered Slick And Slippery Fucking",
                actresses: Array.Empty<string>(),
                genres: new[] { "Beautiful Girl", "Big Tits", "Threesome / Foursome", "Lotion", "POV", "Exclusive Distribution", "VR Exclusive", "High-Quality VR" },
                studio: "Nanpa JAPAN",
                boxArt: "https://awsimgsrc.dmm.com/dig/digital/video/njvr00023/njvr00023ps.jpg",
                cover: "https://awsimgsrc.dmm.com/dig/digital/video/njvr00023/njvr00023pl.jpg",
                releaseDate: DateTime.Parse("2019-07-2"));

            var result = await R18Client.LoadVideo("njvr00023").ConfigureAwait(false);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task TestLoadVideoInvalid()
        {
            var result = await R18Client.LoadVideo("invalid").ConfigureAwait(false);

            Assert.AreEqual(null, result);
        }
    }
}