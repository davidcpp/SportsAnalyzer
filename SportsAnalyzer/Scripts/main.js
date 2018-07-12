var varTitleFontSize, ticksFontSize, legendFontSize, tooltipsFontSize, labelsFontSize;

var timeIntervalsTexts = [];
var goalsInIntervalsPercent = [];
var selectedRounds = [];

var webApiUri = 'api/stats';

var goalsIntervalsURI = webApiUri + "/goalsintervals";
var goalsInIntervalsTitle = "Minutes Intervales of scored goals"
var goalsInIntervalsXLabel = "Time interval [min.]";
var goalsInIntervalsYLabel = "Percent of goals";

var matchGoalsURI = webApiUri + "/matchgoals";
var matchGoalsTitle = "Percent of Matches with a given number of goals";
var matchGoalsXLabel = "Number of goals";
var matchGoalsYLabel = "Percent of matches";

var roundPointsURI = webApiUri + "/roundpoints";
var roundPointsTitle = "Number of points after a given round"
var roundPointsXLabel = "Number of round";
var roundPointsYLabel = "Number of points";

var leagueName, seasonYear;

var color = Chart.helpers.color;
var myChartColors = [
  '#4dc9f6',
  '#f67019',
  '#000000',
  '#6B8E23',
  '#acc236',
  '#B22222',
  '#00a950',
  '#58595b',
  '#FF6347',
  '#00FFFF',
  '#FF00FF',
  '#FFFF00',
  '#0000FF',
  '#00FF00',
  '#FF0000',
  '#8A2BE2',
  '#A52A2A',
  '#6495ED',
  '#BDB76B',
  '#FFA500'
];

var teamStandings = {}

$("#teamsList > option").each(function () {
  var teamName = $(this).text();
  teamStandings[teamName] = {
    points: [],
    opposingTeams: [],
    matchResults: []
  };
});

function GenerateStartDatasetArray(data) {
  if (Array.isArray(data)) {
    return [{
      label: leagueName,
      backgroundColor: color(myChartColors[0]).alpha(0.5).rgbString(),
      borderColor: myChartColors[0],
      borderWidth: 1,
      data: data
    }];
  }
  return [];
}

// Getting data of the Model passed from the Stats view

timeIntervalsTexts = eval($("#mainScript").attr("data-time-intervals-all-text"));
goalsInIntervalsPercent = eval($("#mainScript").attr("data-goals-in-intervals-percent"));
leagueName = eval($("#mainScript").attr("data-league-name"));
seasonYear = eval($("#mainScript").attr("data-season-year"));

function RemoveChartDataset(chart, teamName) {
  for (var i = 0; i < chart.data.datasets.length; i++) {
    if (chart.data.datasets[i].label === teamName) {
      chart.data.datasets.splice(i, 1);
    }
  }
  chart.update();
}

function UpdateChart(chart, chartDisplaySize) {
  UpdateChartFontSizes(chart, chartDisplaySize);
  chart.update();
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

function GetStatsRequestData(teamName) {
  return {
    "TeamName": teamName,
    "LeagueName": leagueName,
    "SeasonYear": seasonYear,
    "Rounds": selectedRounds
  };
}

function AddChartDataset(chart, URI, teamName, id) {
  var statsRequestData = GetStatsRequestData(teamName);
  $.post(URI, statsRequestData, null, "json")
    .done(function (data) {
      var newData;
      if (URI === matchGoalsURI || URI === roundPointsURI) {
        let chartData;

        if (URI === roundPointsURI) {
          chartData = GetRoundPointsData(teamName, data);
          chart.data.labels = chartData.labels;
        }
        else {
          chartData = GetIntegerLabeledData(data);
        }
        newData = chartData.data;
      }
      else {
        newData = data;
      }

      var dataset = {
        label: teamName,
        backgroundColor: color(myChartColors[id]).alpha(0.5).rgbString(),
        borderColor: myChartColors[id],
        borderWidth: 1,
        data: newData,
      };

      if (URI === roundPointsURI) {
        dataset.fill = false;
        dataset.lineTension = 0;
        dataset.borderWidth = 2;
      }

      // calling RemoveChartDataset method in case of delay in receiving results from WebApi
      RemoveChartDataset(chart, teamName);
      chart.data.datasets.push(dataset);
      chart.update();
    })
    .fail(function (jqXHR, textStatus, err) {
      console.log('Error: ' + err);
    });
}

function GetChartDisplaySize(chartName) {
  var canvasChart = document.getElementById(chartName);

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

function OnResizeChart(chart, chartSize) {
  UpdateChart(chart, chartSize);
}

function CreateChart(chartName, title, labels, data, minY, maxY, xAxisLabel, yAxisLabel, typeOfChart,
  yAxisTicksEnding, tooltipTitlePrefix, tooltipLabelEnding) {
  var ctx = $("#" + chartName);
  var chart = new Chart(ctx, {
    // The type of chart we want to create
    type: typeOfChart,
    // The data for our dataset
    data: {
      labels: labels,
      datasets: GenerateStartDatasetArray(data)
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
        text: title
      },
      tooltips: {
        titleFontSize: tooltipsFontSize,
        bodyFontSize: tooltipsFontSize,
        mode: 'index',
        callbacks: {
          title: function (tooltipItems, data) {
            var title = tooltipTitlePrefix + tooltipItems[0].xLabel;
            return title;
          },
          label: function (tooltipItem, data) {
            var label = data.datasets[tooltipItem.datasetIndex].label || '';

            if (label) {
              label += ': ';
            }
            label += tooltipItem.yLabel;
            label += tooltipLabelEnding;
            return label;
          }
        }
      },
      scales: {
        yAxes: [{
          scaleLabel: {
            display: true,
            labelString: yAxisLabel,
            fontColor: window.chartColors.blue,
            fontSize: labelsFontSize
          },
          ticks: {
            beginAtZero: true,
            fontSize: ticksFontSize,
            callback: function (value, index, values) {
              return value + yAxisTicksEnding;
            }
          }
        }],
        xAxes: [{
          scaleLabel: {
            display: true,
            labelString: xAxisLabel,
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

  if (maxY != 0) {
    chart.options.scales.yAxes[0].ticks.suggestedMin = minY;
    chart.options.scales.yAxes[0].ticks.suggestedMax = maxY;
  }

  if (title == roundPointsTitle) {
    chart.options.tooltips.callbacks.label = function (tooltipItem, data) {
      var label = data.datasets[tooltipItem.datasetIndex].label || '';

      if (label) {
        label += ': ';
      }
      label += tooltipItem.yLabel;
      label += tooltipLabelEnding;

      let teamName = data.datasets[tooltipItem.datasetIndex].label;
      let roundNumber = parseInt(tooltipItem.xLabel);
      label += " | " + teamStandings[teamName].opposingTeams[roundNumber]
        + " " + teamStandings[teamName].matchResults[roundNumber];

      return label;
    }
  }

  chart.options.onResize = OnResizeChart;

  chartDisplaySize = GetChartDisplaySize(chartName);
  UpdateChart(chart, chartDisplaySize);
  return chart;
}

$(document).ready(function () {
  window.goalsInIntervalsChart = CreateChart(
    "goalsInIntervalsChartArea",
    goalsInIntervalsTitle,
    timeIntervalsTexts,
    goalsInIntervalsPercent,
    0, 0,
    goalsInIntervalsXLabel,
    goalsInIntervalsYLabel,
    "bar", "%", "Minutes interval: ", "%");

  ConfirmSelectedRounds();
  window.matchGoalsChart = CreateChart(
    "matchGoalsChartArea",
    matchGoalsTitle,
    [],
    [],
    0, 0,
    matchGoalsXLabel,
    matchGoalsYLabel,
    "bar", "%", "Goals number: ", "%");

  GetMatchGoals(window.matchGoalsChart, "*");

  window.roundPointsChart = CreateChart(
    "roundPointsChartArea",
    roundPointsTitle,
    null,
    null,
    0, 20,
    roundPointsXLabel,
    roundPointsYLabel,
    "line", "pts", "Round: ", "pts");
});

function GetGoalsInIntervals(chart, index, teamName) {
  var statsRequestData = GetStatsRequestData(teamName);
  $.post(goalsIntervalsURI, statsRequestData, null, "json")
    .done(function (data) {
      UpdateChartData(chart, data, index);
    })
    .fail(function (jqXHR, textStatus, err) {
      console.log('Error: ' + err);
    });
}

function UpdateChartData(chart, data, index) {
  chart.data.datasets[index].data = data;
  chart.update();
}

$("#teamsList").change(function () {
  $("#teamsList > option").each(function () {
    var teamName = $(this).text();
    var id = $(this).val();

    if ($(this).prop("selected")) {
      AddChartDataset(window.goalsInIntervalsChart, goalsIntervalsURI, teamName, id, );
      AddChartDataset(window.matchGoalsChart, matchGoalsURI, teamName, id);
      AddChartDataset(window.roundPointsChart, roundPointsURI, teamName, id);
    }
    else {
      RemoveChartDataset(window.goalsInIntervalsChart, teamName);
      RemoveChartDataset(window.matchGoalsChart, teamName);
      RemoveChartDataset(window.roundPointsChart, teamName);
    }
  });
});

$("#changeRounds").click(function () {
  ConfirmSelectedRounds();

  for (var i = 0; i < window.goalsInIntervalsChart.data.datasets.length; i++) {
    var dataset = window.goalsInIntervalsChart.data.datasets[i];
    var teamName = "*";

    if (dataset.label !== leagueName) {
      teamName = dataset.label;
    }

    GetGoalsInIntervals(window.goalsInIntervalsChart, i, teamName);
    GetMatchGoals(window.matchGoalsChart, teamName, i);
  }
});

function GetRoundPointsData(teamName, data) {
  var resultArray = [];
  var labels = Object.keys(data);
  var values = Object.values(data);
  var maxLabel = labels[labels.length - 1];

  for (var i = 0; i < labels.length; i++) {
    resultArray[parseInt(labels[i])] = parseInt(values[i].Points);
    teamStandings[teamName].points[parseInt(labels[i])] = values[i].Points;
    teamStandings[teamName].opposingTeams[parseInt(labels[i])] = values[i].OpposingTeams;
    teamStandings[teamName].matchResults[parseInt(labels[i])] = values[i].MatchResult;
  }

  for (var i = 0; i <= maxLabel; i++) {
    labels[i] = i;
    if (resultArray[i] == undefined) {
      resultArray[i] = 0;
      teamStandings[teamName].points[i] = 0;
      teamStandings[teamName].opposingTeams[i] = "";
      teamStandings[teamName].matchResults[i] = "";
    }
  }
  chartData = new ChartData(resultArray, labels);
  return chartData;
}

function GetIntegerLabeledData(data) {
  var resultArray = [];
  var labels = Object.keys(data);
  var values = Object.values(data);
  var maxLabel = labels[labels.length - 1];

  for (var i = 0; i < labels.length; i++) {
    resultArray[parseInt(labels[i])] = parseFloat(values[i]);
  }

  for (var i = 0; i <= maxLabel; i++) {
    labels[i] = i;
    if (resultArray[i] == undefined)
      resultArray[i] = 0;
  }
  chartData = new ChartData(resultArray, labels);
  return chartData;
}

function GetMatchGoals(chart, teamName, index = 0) {
  var statsRequestData = GetStatsRequestData(teamName);
  $.post(matchGoalsURI, statsRequestData, null, "json")
    .done(function (data) {
      var chartData = GetIntegerLabeledData(data);
      if (index === 0) {
        chart.data.labels = chartData.labels;
      }
      UpdateChartData(chart, chartData.data, index)
    })
    .fail(function (jqXHR, textStatus, err) {
      console.log('Error: ' + err);
    });
}

class ChartData {
  constructor(data, labels) {
    this.data = data;
    this.labels = labels;
  }
}