g_model_warning_check = [
  {
    message:"Read/Write enabled",
    check_func:function( data ){
      return data.isReadable;
    }
  },
  {
    message:"Optimize disable",
    check_func:function( data ){
       return !data.optimizeMesh;
    }
  }
];


$(document).ready( function(){
  var result = getWarningSummaryData( g_model_report ,  g_model_warning_check );
  writeToWarningField( result );
});
