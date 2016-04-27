
g_audio_warning_check = [
  {
    message:"Too high sampling rate audio",
    check_func:function( data ){
      return (data.sampleRateOverride > 44100);
    }
  }
];


$(document).ready( function(){
  var result = getWarningSummaryData( g_audio_report ,  g_audio_warning_check );
  writeToWarningField( result );
});
