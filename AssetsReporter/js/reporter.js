
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
  return val.replace(/ /g,'_').replace(/\./g,'_').replace(/\//g,'_');
}

function setupCheckBox( data,thStr ,headStr,writeTo ){
  var length = data.length;
  var i = 0;
  var html = "<table>";
  html += "<tr><th>" + thStr + "</th><th>Check</th><th>Hit num</th></tr>";
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

function getConditionList( idstr ){
   var list = $("#" +idstr + " input.cond_value");
   var idx = 0;
   var ret = [];
   list.each( function(){
     var tmp = $(this).val();
     if( tmp ){
       ret[idx] = tmp; ++ idx;
     }
   } );

   return ret;
}

function isIncludeFileName( path , cond_list ){
   if( !cond_list || cond_list.length == 0 ){ return true; }
   var length = cond_list.length;
   for( var i = 0; i < length; ++ i ){
     if( path.indexOf(cond_list[i] ) >= 0 ){
       return true;
     }
   }
   return false;
}

function isExcludeFileName( path , cond_list ){
   if( !cond_list || cond_list.length == 0 ){ return false; }
   var length = cond_list.length;
   for( var i = 0; i < length; ++ i ){
     if( path.indexOf(cond_list[i] ) >= 0 ){
       return true;
     }
   }
   return false;
}


function getWarningSummaryData( data , check ){
  var data_length = data.length;
  var check_length = check.length;
  var check_result = {};

  for( var i = 0 ; i < data_length ; ++ i ){
    for( var j = 0 ; j < check_length ; ++ j ){
      if( check[j].check_func(data[i]) ){
        if(check_result[ check[j].message ] == undefined){
          check_result[ check[j].message ] = 1;
        }else{
          ++ check_result[ check[j].message ];
        }
      }
    }
  }
  return check_result;
}
function writeToWarningField( result ){
  // report
  if( result ){
    var str = '<h2 class="warning">Warning</h2>';
    str += '<table>';
    str += '<tr><th>Item</th><th>HitNum</ht></tr>';
    for( var idx in result ){
      str += '<tr><td>' + idx + "</td><td>" + result[idx] + '</td></tr>';
    }
    str += '</table>';
    str += '<br />';
    $("#warning_field").html(str);
  }
}

$(document).ready( function(){
  var date_str = "report : " + g_report_at;
  var platform_str = g_report_platform;
  if( !g_report_platform ){
     platform_str = "platform : none";
  }else{
     platform_str = "platform : " + platform_str;
  }

  $("#report_at").html(date_str);
  $("#platform").html(platform_str);
});

$(document).on("click",".add_cond",function() {
  var str = '<div><input class="cond_value" type="text" size="80"><input type="button" class="del_cond" value="delete"></div>';
  $(this).before(str);
});
$(document).on("click",".del_cond",function() {
  $(this).parent('div').remove();
});


