
g_texture_warning_check = [
  {
    message:"Read/Write enabled texture",
    check_func:function( data ){
      return data.isReadable;
    }
  },
  {
    message:"Not pow2 size texture",
    check_func:function( data ){
      return !data.isPow2;
    }
  },
  {
    message:"Too small size texture",
    check_func:function( data ){
      return (data.width < 64 || data.height < 64 );
    }
  }
];


$(document).ready( function(){
  var result = getWarningSummaryData( g_texture_report ,  g_texture_warning_check );
  writeToWarningField( result );
});
