var varTitleFontSize, ticksFontSize, legendFontSize, tooltipsFontSize, labelsFontSize;
var timeIntervalsAllText = "";
var timeIntervalsTexts = [];
var goalsInIntervalsPercent;
var selectedRounds = [];
var webApiUri = 'api/stats';

var myChartColors = [window.chartColors.blue,
window.chartColors.red,
window.chartColors.orange,
window.chartColors.yellow];

// Getting data of the Model passed from the Stats view

timeIntervalsAllText = $("#mainScript").attr("data-time-intervals-all-text");
goalsInIntervalsPercent = eval($("#mainScript").attr("data-goals-in-intervals-percent"));


function RemoveChartDataset(teamName) {
  for (var i = 0; i < window.myChart.data.datasets.length; i++) {
    if (window.myChart.data.datasets[i].label === teamName) {
      window.myChart.data.datasets.splice(i, 1);
    }
  }
  window.myChart.update();
}

function UpdateChart(chart, chartDisplaySize) {
  UpdateChartFontSizes(chart, chartDisplaySize);
  window.myChart.update();
}

function ConfirmSelectedRounds() {
  var roundsSize = selectedRounds.length;
  for (var i = 0; i < roundsSize; i++) {
    selectedRounds.pop();
  }
  $("#roundsList :selected").each(function () {
    selectedRounds.push($(this).val());
  });
}

function AddChartDataset(teamName, id) {
  $.post(webApiUri, { "Rounds": selectedRounds, "TeamName": teamName }, null, "json")
    .done(function (data) {

      var dataset = {
        label: teamName,
        backgroundColor: color(myChartColors[id]).alpha(0.5).rgbString(),
        borderColor: myChartColors[id],
        borderWidth: 1,
        data: data
      };

      // calling RemoveChartDataset method in case of delay in receiving results from WebApi
      RemoveChartDataset(teamName);
      window.myChart.data.datasets.push(dataset);
      window.myChart.update();
    })
    .fail(function (jqXHR, textStatus, err) {
      console.log('Error: ' + err);
    });
}

function GetChartDisplaySize() {
  var canvasChart = document.getElementById('myChart');

  chartDisplaySize = {
    width: parseFloat(canvasChart.style.width),
    height: parseFloat(canvasChart.style.height)
  };
  return chartDisplaySize;
}

function UpdateChartFontSizes(chart, chartSize) {
  if (typeof chartSize !== "undefined") {
    legendFontSize = Math.round(0.01 * chartSize.width + 8);
    varTitleFontSize = Math.round(0.015 * chartSize.width + 8);
    tooltipsFontSize = Math.round(0.005 * chartSize.width + 10);
    labelsFontSize = Math.round(0.005 * chartSize.width + 10);
    ticksFontSize = Math.round(0.005 * chartSize.width + 10);
  }

  if (typeof chart !== "undefined") {
    chart.options.title.fontSize = varTitleFontSize;
    chart.options.legend.labels.fontSize = legendFontSize;
    chart.options.tooltips.titleFontSize = tooltipsFontSize;
    chart.options.tooltips.bodyFontSize = tooltipsFontSize;
    chart.options.scales.yAxes[0].scaleLabel.fontSize = labelsFontSize;
    chart.options.scales.xAxes[0].scaleLabel.fontSize = labelsFontSize;
    chart.options.scales.yAxes[0].ticks.fontSize = ticksFontSize;
    chart.options.scales.xAxes[0].ticks.fontSize = ticksFontSize;
  }
}

function PrepareChartData() {
  color = Chart.helpers.color;
  timeIntervalsAllText = timeIntervalsAllText.substring(1, timeIntervalsAllText.length - 1);
  timeIntervalsAllText = timeIntervalsAllText.replace(new RegExp('&quot;', 'g'), '');
  timeIntervalsTexts = timeIntervalsAllText.split(",");
}

function OnResizeChart(chart, chartSize) {
  UpdateChart(chart, chartSize);
}

function CreateChart() {
  PrepareChartData();

  var ctx = $("#myChart");
  window.myChart = new Chart(ctx, {
    // The type of chart we want to create
    type: 'bar',
    // The data for our dataset
    data: {
      labels: timeIntervalsTexts,
      datasets: [{
        label: 'Scottish Premier League',
        backgroundColor: color(myChartColors[0]).alpha(0.5).rgbString(),
        borderColor: myChartColors[0],
        borderWidth: 1,
        data: goalsInIntervalsPercent
      }]
    },
    // Configuration options go here
    options: {
      responsive: true,
      maintainAspectRatio: false,
      legend: {
        position: 'top',
        labels: {
          fontSize: legendFontSize,
          boxWidth: 30
        }
      },
      title: {
        display: true,
        fontSize: varTitleFontSize,
        fontColor: window.chartColors.black,
        text: 'Minutes Intervales of scored goals'
      },
      tooltips: {
        titleFontSize: tooltipsFontSize,
        bodyFontSize: tooltipsFontSize,
        callbacks: {
          label: function (tooltipItem, data) {
            var label = data.datasets[tooltipItem.datasetIndex].label || '';

            if (label) {
              label += ': ';
            }
            label += tooltipItem.yLabel;
            label += "%";
            return label;
          }
        }
      },
      scales: {
        yAxes: [{
          scaleLabel: {
            display: true,
            labelString: 'Percent of goals',
            fontColor: window.chartColors.blue,
            fontSize: labelsFontSize
          },
          ticks: {
            beginAtZero: true,
            fontSize: ticksFontSize,
            callback: function (value, index, values) {
              return value + '%';
            }
          }
        }],
        xAxes: [{
          scaleLabel: {
            display: true,
            labelString: 'Time intervals [min.]',
            fontColor: window.chartColors.blue,
            fontSize: labelsFontSize
          },
          ticks: {
            fontSize: ticksFontSize
          },
          gridLines: {
            offsetGridLines: true
          }
        }]
      }
    }
  });
  window.myChart.options.onResize = OnResizeChart;

  chartDisplaySize = GetChartDisplaySize();
  UpdateChart(window.myChart, chartDisplaySize);
}

$(document).ready(function () {
  CreateChart();
  ConfirmSelectedRounds();
});

$(".form-check-input").change(function () {
  var teamName = $("label[for='" + $(this).attr('id') + "']").text();
  var id = $(this).val();

  if ($(this).prop("checked")) {
    AddChartDataset(teamName, id);
  }
  else {
    RemoveChartDataset(teamName);
  }
});

function GetChartData(index, teamName) {
  $.post(webApiUri, { "Rounds": selectedRounds, "TeamName": teamName }, null, "json")
    .done(function (newData) {
      window.myChart.data.datasets[index].data = newData;
      window.myChart.update();
    })
    .fail(function (jqXHR, textStatus, err) {
      console.log('Error: ' + err);
    });
}

$("#changeRounds").click(function () {
  ConfirmSelectedRounds();

  for (var i = 0; i < window.myChart.data.datasets.length; i++) {
    var datasetData = window.myChart.data.datasets[i].data;

    var teamName = "*";
    if (window.myChart.data.datasets[i].label !== 'Scottish Premier League') {
      var teamName = window.myChart.data.datasets[i].label;
    }

    GetChartData(i, teamName);
  }
});