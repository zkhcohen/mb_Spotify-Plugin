﻿using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicBeePlugin
{

    public partial class Plugin
    {

        private static SpotifyClient _spotify;
        private static int _auth, _num, _trackMissing = 0;
        private static bool _trackLIB, _albumLIB, _artistLIB = false;
        private static string _title, _album, _artist, _trackID, _albumID, _artistID, _imageURL;
        private static string _clientID = "9076681768d94feda885a7b5eced926d";


        static async void SpotifyWebAuth()
        {
            var (verifier, challenge) = PKCEUtil.GenerateCodes(120);

            var loginRequest = new LoginRequest(
                new Uri("http://localhost:5000/callback"), _clientID, LoginRequest.ResponseType.Code)
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] { Scopes.UserLibraryModify, Scopes.UserFollowModify, Scopes.UserFollowRead, Scopes.UserLibraryRead }
            };
            var uri = loginRequest.ToUri();

            var server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

            server.PkceReceived += async (sender, response) =>
            {
                await server.Stop();

                var initialResponse = await new OAuthClient().RequestToken(
                  new PKCETokenRequest(_clientID, response.Code, server.BaseUri, verifier)
                );

                var authenticator = new PKCEAuthenticator(_clientID, initialResponse);

                var config = SpotifyClientConfig.CreateDefault()
                  .WithAuthenticator(authenticator);
                _spotify = new SpotifyClient(config);
            };
            await server.Start();

            try
            {
                BrowserUtil.Open(uri);
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to open URL, manually open: {0}", uri);
            }

            _auth = 1;
        }

        public async Task<FullTrack> TrackSearch()
        {

            try
            {
                var track = await _spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, _searchTerm));
                _title = Truncate(track.Tracks.Items[_num].Name, largeBold);
                _artist = Truncate(string.Join(", ", from item in track.Tracks.Items[_num].Artists select item.Name), smallRegular);
                _album = Truncate(track.Tracks.Items[_num].Album.Name, smallRegular);
                _trackID = track.Tracks.Items[_num].Id;
                _albumID = track.Tracks.Items[_num].Album.Id;
                _artistID = track.Tracks.Items[_num].Artists[0].Id;
                _imageURL = track.Tracks.Items[_num].Album.Images[0].Url;
                _trackMissing = 0;
                return null;
            }
            catch (APIUnauthorizedException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
                return null;
            }
            catch (APIException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
                return null;
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Song not found!");
                _trackMissing = 1;
                return null;
            }

        }

        public void SaveTrack()
        {
            try
            {
                var track = new LibrarySaveTracksRequest(new List<string> { _trackID });
                _spotify.Library.SaveTracks(track);
                Console.WriteLine("Track Saved.");
            }
            catch (APIUnauthorizedException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (APIException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Song not found!");
            }

        }

        public void SaveAlbum()
        {
            try
            {
                var album = new LibrarySaveAlbumsRequest(new List<string> { _albumID });
                _spotify.Library.SaveAlbums(album);
                Console.WriteLine("Album Saved.");
            }
            catch (APIUnauthorizedException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (APIException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Song not found!");
            }
            
        }

        public void FollowArtist()
        {
            try
            {
                var artist = new FollowRequest(FollowRequest.Type.Artist, new List<string> { _artistID });
                _spotify.Follow.Follow(artist);
                Console.WriteLine("Artist Followed.");
            }
            catch (APIUnauthorizedException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (APIException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Song not found!");
            }
            
        }



        public void RemoveTrack()
        {
            try
            {
                var track = new LibraryRemoveTracksRequest(new List<string> { _trackID });
                _spotify.Library.RemoveTracks(track);
                Console.WriteLine("Track Unsaved.");
            }
            catch (APIUnauthorizedException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (APIException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Song not found!");
            }
            
        }

        public void RemoveAlbum()
        {
            try
            {
                var album = new LibraryRemoveAlbumsRequest(new List<string> { _albumID });
                _spotify.Library.RemoveAlbums(album);
                Console.WriteLine("Album Unsaved.");
            }
            catch (APIUnauthorizedException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (APIException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Song not found!");
            }
            
        }

        public void UnfollowArtist()
        {
            try
            {
                var artist = new UnfollowRequest(UnfollowRequest.Type.Artist, new List<string> { _artistID });
                _spotify.Follow.Unfollow(artist);
                Console.WriteLine("Artist Unfollowed.");
            }
            catch (APIUnauthorizedException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (APIException e)
            {
                Console.WriteLine("Error Status: " + e.Response);
                Console.WriteLine("Error Msg: " + e.Message);
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Song not found!");
            }
            
        }



        public Boolean CheckTrack(string id)
        {
            var tracks = new LibraryCheckTracksRequest(new List<String> { id });

            List<bool> tracksSaved = _spotify.Library.CheckTracks(tracks).Result;
            if (tracksSaved.ElementAt(0))
            {
                _trackLIB = true;
                return true;
            }
            else
            {
                _trackLIB = false;
                return false;
            }
        }

        public Boolean CheckAlbum(string id)
        {

            var albums = new LibraryCheckAlbumsRequest(new List<String> { id });

            List<bool> albumsSaved = _spotify.Library.CheckAlbums(albums).Result;
            if (albumsSaved.ElementAt(0))
            {
                _albumLIB = true;
                return true;
            }
            else
            {
                _albumLIB = false;
                return false;
            }


        }

        public Boolean CheckArtist(string id)
        {

            var artist = new FollowCheckCurrentUserRequest(FollowCheckCurrentUserRequest.Type.Artist, new List<string> { id });

            List<bool> artistFollowed = _spotify.Follow.CheckCurrentUser(artist).Result;
            if (artistFollowed.ElementAt(0))
            {
                _artistLIB = true;
                return true;
            }
            else
            {
                _artistLIB = false;
                return false;
            }
        }


    }
}
