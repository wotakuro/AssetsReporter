
function getCheckCondition( data , headerStr){
   var res = {};
   length = data.length;
   for( i = 0 ; i < length ; ++ i ){
     var t = data[i].val;
     res[ t ] = $("#" + headerStr + convertCheckBoxValue(t) ).prop('checked');
   }
   return res;
}

function convertCheckBoxValue( val ){
  return val.replace(/ /g,'_').replace(/./g,'_');
}

function setupCheckBox( data,thStr ,headStr,writeTo ){
  var length = data.length;
  var i = 0;
  var html = "<table>";
  html += "<tr><th>" + thStr + "</th><th>チェック</th><th>該当数</th></tr>";
  for( i = 0; i < length ; ++ i ){
    html += '<tr>';
    html += '<td>'+ data[i].val + '</td>';
    html += '<td>' + '<input type="checkbox" id="'+ headStr + convertCheckBoxValue( data[i].val ) + '" checked/>'+ '</td>';
    html += '<td>'+ data[i].cnt + '</td>';
    html += '</tr>';
  }
  html += "</table>";
  $(writeTo).html(html);
}

$(document).ready( function(){
  var date_str = "report : " + g_report_at;
  var platform_str = g_report_platform;
  if( !g_report_platform ){
     platform_str = "platform : 指定なし";
  }else{
     platform_str = "platform : " + platform_str;
  }

  $("#report_at").html(date_str);
  $("#platform").html(platform_str);
});
