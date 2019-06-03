using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MusicBeePlugin
{

    public partial class Plugin
    {

        private static SpotifyWebAPI _spotify;
        private static int _auth, _num, _trackMissing = 0;
        private static bool _trackLIB, _albumLIB, _artistLIB, _runOnce = false;
        private static string _title, _album, _artist, _trackID, _albumID, _artistID, _imageURL;



        static async void SpotifyWebAuth(bool autoRefresh)
        {
            ImplicitGrantAuth auth =
                new ImplicitGrantAuth("777824a07eeb4312972ff5fcec54c565", "http://localhost:4002", "http://localhost:4002", Scope.UserLibraryModify | Scope.UserFollowModify | Scope.UserFollowRead | Scope.UserLibraryRead);
            auth.AuthReceived += async (sender, payload) =>
            {

                auth.Stop(); // `sender` is also the auth instance
                _spotify = new SpotifyWebAPI() { TokenType = payload.TokenType, AccessToken = payload.AccessToken };

                
            };
            auth.Start(); // Starts an internal HTTP Server

            auth.OpenBrowser(autoRefresh);

            _auth = 1;
            _runOnce = true;
        }



        public FullTrack TrackSearch()
        {
            
            
            SearchItem track = _spotify.SearchItems(_searchTerm, SearchType.Track, 10);

            if (track.HasError())
            {

                _trackMissing = 1;
                Console.WriteLine("Error Status: " + track.Error.Status);
                Console.WriteLine("Error Msg: " + track.Error.Message);

            }
            else if (track.Tracks.Total >= 1)
            {
                _title = Truncate(track.Tracks.Items[_num].Name, largeBold);
                _artist = Truncate(string.Join(", ", from item in track.Tracks.Items[_num].Artists select item.Name), smallRegular);
                _album = Truncate(track.Tracks.Items[_num].Album.Name, smallRegular);
                _trackID = track.Tracks.Items[_num].Id;
                _albumID = track.Tracks.Items[_num].Album.Id;
                _artistID = track.Tracks.Items[_num].Artists[0].Id;
                _imageURL = track.Tracks.Items[_num].Album.Images[0].Url;
                
            }
            else
            {

                _trackMissing = 1;

            }
            
            return null;
        }
        


        public void SaveTrack()
        {

            ErrorResponse response = _spotify.SaveTrack(_trackID);
            if (!response.HasError())
                MessageBox.Show("Track Saved.");
            else
                MessageBox.Show(response.Error.Message);

        }

        public void SaveAlbum()
        {

            ErrorResponse response = _spotify.SaveAlbum(_albumID);
            if (!response.HasError())
                MessageBox.Show("Album Saved.");
            else
                MessageBox.Show(response.Error.Message);

        }

        public void FollowArtist()
        {
            ErrorResponse response = _spotify.Follow(FollowType.Artist, _artistID);
            if (!response.HasError())
                MessageBox.Show("Artist Followed.");
            else
                MessageBox.Show(response.Error.Message);

        }



        public void RemoveTrack()
        {

            ErrorResponse response = _spotify.RemoveSavedTracks(new List<string> { _trackID });
            if (!response.HasError())
                MessageBox.Show("Track Unsaved.");
            else
                MessageBox.Show(response.Error.Message);

        }

        public void RemoveAlbum()
        {
            ErrorResponse response = _spotify.RemoveSavedAlbums(new List<string> { _albumID});
            if (!response.HasError())
                MessageBox.Show("Album Unsaved.");
            else
                MessageBox.Show(response.Error.Message);

        }

        public void UnfollowArtist()
        {
            ErrorResponse response = _spotify.Unfollow(FollowType.Artist, _artistID);
            if (!response.HasError())
                MessageBox.Show("Artist Unfollowed.");
            else
                MessageBox.Show(response.Error.Message);

        }



        public Boolean CheckTrack(string id)
        {
            ListResponse<bool> tracksSaved = _spotify.CheckSavedTracks(new List<String> { id });
            if (tracksSaved.List[0])
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
            //API Code which doesn't currently work correctly.
            //ListResponse<bool> albumsSaved = _spotify.CheckSavedAlbums(new List<String> { id });
            //if (albumsSaved.List[0])

            
            foreach (string line in File.ReadLines(_savedAlbumsPath))
            {
                if (line.Contains(_albumID))
                {
                    _albumLIB = true;
                    return true;
                }
                else
                {
                    _albumLIB = false;
                }
            }
            

            if (_albumLIB)
            { return true; }
            else
            { return false; }
        


    }

        public Boolean CheckArtist(string id)
        {
            ListResponse<Boolean> response = _spotify.IsFollowing(FollowType.Artist, id);
            if (response.List[0] == true)
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

        // Workaround for Spotify API "Check-Users-Saved-Albums" Endpoint bug.
        public void GenerateAlbumList()
        {

            int offset = 0;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(_savedAlbumsPath))
            {
                

                while (offset != -1)
                {
                    
                    Paging<SavedAlbum> savedAlbums = _spotify.GetSavedAlbums(50, offset);
                    savedAlbums.Items.ForEach(album => file.WriteLine(album.Album.Id));

                    if (savedAlbums.Next == null)
                    {
                        offset += -1;
                        break;
                    }
                    else
                    {
                        offset += 50;
                    }

                }
                file.Close();
            }

        }


    }
}
