﻿//
// Copyright (C) 2019  Carl Reinke
//
// This file is part of Hypersonic.
//
// This program is free software: you can redistribute it and/or modify it under the terms of the
// GNU General Public License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program.  If
// not, see <https://www.gnu.org/licenses/>.
//
using Hypersonic.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using Xunit;
using static Hypersonic.Tests.Helpers;

namespace Hypersonic.Tests
{
    public static partial class RestApiQueriesTests
    {
        public static class SetPlaylistSongsAsyncTests
        {
            [Fact]
            public static void TestSetPlaylistSongsAsync()
            {
                var dbConnection = OpenSqliteDatabase();

                var dbContextOptionsBuilder = new DbContextOptionsBuilder<MediaInfoContext>()
                    .DisableClientSideEvaluation()
                    .UseSqlite(dbConnection);

                using (var dbContext = new MediaInfoContext(dbContextOptionsBuilder.Options))
                {
                    var random = new RandomPopulator(dbContext);
                    var user = random.AddUser();
                    var library = random.AddLibrary();
                    var directory = random.AddDirectory(library);
                    var trackFile = random.AddFile(directory);
                    var artist = random.AddArtist();
                    var album = random.AddAlbum(artist);
                    var track1 = random.AddTrack(trackFile, artist, album);
                    var track2 = random.AddTrack(trackFile, artist, album);
                    var playlist = random.AddPlaylist(user);
                    var playlistTrack = random.AddPlaylistTrack(playlist, track1, 0);
                    dbContext.SaveChanges();

                    RestApiQueries.SetPlaylistSongsAsync(dbContext, user.UserId, playlist.PlaylistId, new[] { track2.TrackId }, CancellationToken.None).GetAwaiter().GetResult();
                    dbContext.SaveChanges();

                    Assert.False(dbContext.PlaylistTracks.Any(pt => pt.PlaylistId == playlist.PlaylistId && pt.TrackId == track1.TrackId));
                    Assert.True(dbContext.PlaylistTracks.Any(pt => pt.PlaylistId == playlist.PlaylistId && pt.TrackId == track2.TrackId));
                }
            }
        }
    }
}
