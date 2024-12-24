﻿function deleteModule() {
    if (!confirm('确定要删除此模块吗？数据可能永久丢失，请注意备份！')) {
        return false;
    }

    document.getElementById('form_delete').submit();
    return true;
}

function submitFunction(id) {
    var dataInputs = $('[id^=functionForm_Editor_' + id + '_]');
    var jsonData = {};
    var selectionDataIndex = 0;
    for (var i = 0; i < dataInputs.length; i++) {

        var ele = dataInputs[i];
        var tagName = ele.tagName.toLowerCase();
        var obj = $(ele);

        var dataKey = obj.data('funcname');

        if (jsonData[dataKey] === undefined) {
            selectionDataIndex = 0;
        }

        var val;
        if ((tagName === 'input' && obj.attr('type') === 'text') || tagName === 'textara') {
            val = obj.val();
        } else if (tagName === 'select') {
            selectionDataIndex++;
            val = obj.val();
        } else if (tagName === 'input' && obj.attr('type') === 'checkbox') {
            selectionDataIndex++;
            val = obj.prop('checked') ? obj.val() : '';
        }
        else {
            alert('未知的编辑器类型：' + tagName);
            return false;
        }

        if (selectionDataIndex === 0) {
            //Normal input e.g. TextBox
            jsonData[dataKey] = val;

        } else {
            //CheckBox or Dropdownlad ... SelectionItems
            if (jsonData[dataKey] === undefined) {
                jsonData[dataKey] = {
                    SelectedValues: []
                };
            }
            jsonData[dataKey].SelectedValues.push(val);
        }
    }
    $('#xscfFunctionParams_' + id).val(JSON.stringify(jsonData));

    $('#functionForm_' + id).ajaxSubmit(function (data) {

        var downloadArea = $('#result_small_modal_download_log');
        if ((data.log || '').length > 0 && (data.tempId || '').length > 0) {
            downloadArea.show();
            $('#result_small_modal_download_log_link').attr('href', '?handler=Log&tempId=' + data.tempId);
        } else {
            downloadArea.hide();
        }

        if (!data.success) {
            showModal('遇到错误', '错误信息', data.msg);
            return;
        }

        if (data.msg && data.msg.indexOf('http://') !== -1 || data.msg.indexOf('https://') !== -1) {
            showModal('执行成功', '收到网址，点击下方打开<br />（此链接由第三方提供，请注意安全）：', '<i class="fa fa-external-link"></i> <a href="' + data.msg + '" target="_blank">' + data.msg + '</a>');
        }
        else {
            showModal('执行成功', '返回信息', data.msg, true);
        }
    });
    return false;
}

function showModal(title, subtitle, contentHtml, rawHTML) {
    $('#result_small_modal_title').html(title);
    $('#result_small_modal_subtitle').html(subtitle);

    var html = rawHTML ? HTMLDecode(contentHtml || '') : (contentHtml || '');
    $('#result_small_modal_content').html(html.replace(/\n/g, '<br />'));

    $('#result_small_modal').modal();

}

function HTMLDecode(text) {
    var temp = document.createElement("div");
    temp.innerHTML = text;
    var output = temp.innerText || temp.textContent;
    temp = null;
    return output;
}
