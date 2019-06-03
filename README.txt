DESCRIPTION:

This plugin integrates Spotify with your MusicBee library. You can add albums and tracks and follow artists directly from MusicBee, through the plugin interface.


INSTALLATION:

Just extract the contents of the "plugins" folder to your MusicBee Plugins directory. 

Let me know if you encounter any bugs - "ZKHCOHEN" on the MusicBee Forums.


KNOWN ISSUES:

Performance issues due to a workaround I implemented because of a bug in the Spotify API. Adding/removing albums is slow. The first track you play after enabling the add-in is slow to load.
You have to re-authenticate hourly due to a limitation with the Spotify API. I plan to do this silently in the future.