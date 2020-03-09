'use strict';

// Chart objects
let goalsInIntervalsChart = {},
  matchGoalsChart = {},
  roundPointsChart = {};

// Variables/objects from model/API
let timeIntervalsTexts = eval($('#mainScript').attr('data-time-intervals-all-text')),
  goalsInIntervalsPercent = eval($('#mainScript').attr('data-goals-in-intervals-percent')),
  matchGoalsPct = eval($('#mainScript').attr('data-match-goals')),
  matchGoalsLabels = Array.from(Array(matchGoalsPct.length).keys()),
  leagueName = eval($('#mainScript').attr('data-league-name')),
  seasonYear = eval($('#mainScript').attr('data-season-year')),
  teamStandings = {};

// Variables/objects from user input
let selectedRounds = [],
  firstTeam = {},
  // Order of the adding teams to the chart or presence on the chart during teams change
  teamsInSelectionOrder = {};

// Variables to designate team selected as first from #teamsList ListBox
let chartFirstTeamName = '',
  isFirstTeamRemoved = false,
  isNewFirstTeamSelected = false;

// Variables for charts titles and WebAPI
const webApiUri = 'api/stats';
const goalsInIntervalsTitle = 'Minutes Intervales of scored goals',
  goalsInIntervalsTooltipTitle = 'Interval',
  goalsInIntervalsURI = webApiUri + '/goalsintervals',
  goalsInIntervalsXLabel = 'Time interval [min.]',
  goalsInIntervalsYLabel = 'Percent of goals';

const matchGoalsTitle = 'Percent of Matches with a given number of goals',
  matchGoalsTooltipTitle = 'Goals',
  matchGoalsURI = webApiUri + '/matchgoals',
  matchGoalsXLabel = 'Number of goals',
  matchGoalsYLabel = 'Percent of matches';

const roundPointsLabels = Array.from(Array(20).keys(), x => x + 1),
  roundPointsTitle = 'Number of points after a given round',
  roundPointsTooltipTitle = 'Round',
  roundPointsURI = webApiUri + '/roundpoints',
  roundPointsXLabel = 'Number of round',
  roundPointsYLabel = 'Number of points';

// Variables/objects for charts style
let labelsFontSize, legendFontSize, ticksFontSize, titleFontSize, tooltipsFontSize;
const alphaFactor = 0.65;
const color = Chart.helpers.color;
const myChartColors = [
  '#4DC9F6',
  '#194071',
  '#111111',
  '#FFF933',
  '#ACC236',
  '#B22222',
  '#00FA9A',
  '#FF69B4',
  '#FF5347',
  '#EEC98C',
  '#9F9F9F',
  '#F59203',
  '#808000',
];

const chartDefaultConfig = {
  responsive: true,
  maintainAspectRatio: false,
  legend: {
    position: 'top',
    labels: {
      fontSize: legendFontSize,
      boxWidth: 30,
    }
  },
  title: {
    display: true,
    fontSize: titleFontSize,
    fontColor: window.chartColors.black,
    text: 'Default Chart',
  },
  tooltips: {
    titleFontSize: tooltipsFontSize,
    bodyFontSize: tooltipsFontSize,
    mode: 'index',
  },
  scales: {
    yAxes: [{
      scaleLabel: {
        display: true,
        fontColor: window.chartColors.blue,
        fontSize: labelsFontSize,
      },
      ticks: {
        beginAtZero: true,
        fontSize: ticksFontSize,
      }
    }],
    xAxes: [{
      scaleLabel: {
        display: true,
        fontColor: window.chartColors.blue,
        fontSize: labelsFontSize,
      },
      ticks: {
        fontSize: ticksFontSize,
      }
    }]
  },
};

$('#teamsList > option').each((index, teamItem) => {
  const teamName = $(teamItem).text();

  // teamStandings - standings for teamName in consecutive rounds
  teamStandings[teamName] = {
    points: [],
    tablePositions: [],
    opponentCrests: [],
    opposingTeams: [],
    matchResults: [],
    matchDates: [],
  };
});

updateSelectedRounds();

// TODO: Change to plugin applied only for roundPointsChart
Chart.plugins.register({
  afterDatasetsDraw: addTablePositionsOnChart
});

function addTablePositionsOnChart(chart) {
  let ctx = chart.ctx;

  // Draw the team's position in the table only on the roundPointsChart
  if (chart.options.title.text != roundPointsTitle)
    return;

  // TODO: Remove forEach and run code only for dataset.label == chartFirstTeamName or apply .filter method
  chart.data.datasets.forEach((dataset, i) => {
    // Table position will be shown only for the first team on the chart
    if (dataset.label != chartFirstTeamName)
      return;

    const teamName = dataset.label,
      meta = chart.getDatasetMeta(i);

    if (!meta.hidden) {
      meta.data.forEach((element, index) => {
        let dataString = teamStandings[teamName].tablePositions[index];

        // TODO: dataString = '-' for undefined
        if (dataString == undefined)
          return;

        dataString = dataString.toString();
        // Draw the text in black, with the specified font
        ctx.fillStyle = 'rgb(0, 0, 0)';

        const fontSize = 14,
          fontStyle = 'normal',
          fontFamily = 'Arial';
        ctx.font = Chart.helpers.fontString(fontSize, fontStyle, fontFamily);

        // Make sure alignment settings are correct
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';

        const padding = 10,
          position = element.tooltipPosition();
        ctx.fillText(dataString, position.x, position.y - (fontSize / 2) - padding);
      });
    }
  });
}

function generateStartDatasetArray(data) {
  if (Array.isArray(data)) {
    return [{
      label: leagueName,
      backgroundColor: color(myChartColors[0]).alpha(alphaFactor).rgbString(),
      borderColor: myChartColors[0],
      borderWidth: 1,
      data: data,
    }];
  }
  return [];
}

function removeChartDataset(chart, teamName) {
  chart.data.datasets = chart.data.datasets.filter(dataset => dataset.label !== teamName);
  chart.update();
}

function updateChart(chart, chartDisplaySize) {
  updateChartFontSizes(chart, chartDisplaySize);
  chart.update();
}

function updateSelectedRounds() {
  selectedRounds = [];

  $('#roundsList :selected').each((index, roundItem) => {
    selectedRounds.push($(roundItem).val());
  });
}

function getStatsRequestData(teamName) {
  return {
    teamName,
    leagueName,
    seasonYear,
    rounds: selectedRounds,
  };
}

function addChartDataset(chart, URI, teamName, id) {
  const statsRequestData = getStatsRequestData(teamName);

  $.post(URI, statsRequestData, null, 'json')
    .done((data) => {

      let dataset = {
        label: teamName,
        backgroundColor: color(myChartColors[id]).alpha(alphaFactor).rgbString(),
        borderColor: myChartColors[id],
        borderWidth: 1,
        data: [],
      };

      if (URI === roundPointsURI) {
        let chartData = extractRoundPointsData(teamName, data);

        // Update labels of the chart when data for new team has more labels
        if (chartData.labels.length > chart.data.labels.length) {
          chart.data.labels = chartData.labels;
        }
        dataset.data = chartData.data;
        dataset.fill = false;
        dataset.lineTension = 0;
        dataset.borderWidth = 2;
        // Add crests of opponents if dataset will be first on the chart
        if (teamName == chartFirstTeamName) {
          dataset.pointStyle = teamStandings[teamName].opponentCrests;
        }
      }
      else {
        dataset.data = data;
      }

      // Call removeChartDataset method for case of delay in receiving results from WebAPI
      removeChartDataset(chart, teamName);
      chart.data.datasets.push(dataset);
      chart.update();
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

function getChartDisplaySize(chartName) {
  const canvasChart = document.getElementById(chartName);

  let chartDisplaySize = {
    width: parseFloat(canvasChart.style.width),
    height: parseFloat(canvasChart.style.height),
  };
  return chartDisplaySize;
}

function updateChartFontSizes(chart, chartSize) {
  if (typeof chartSize !== 'undefined') {
    legendFontSize = Math.round(0.01 * chartSize.width + 8);
    titleFontSize = Math.round(0.015 * chartSize.width + 8);
    tooltipsFontSize = Math.round(0.005 * chartSize.width + 10);
    labelsFontSize = Math.round(0.005 * chartSize.width + 10);
    ticksFontSize = Math.round(0.005 * chartSize.width + 10);
  }

  if (typeof chart !== 'undefined') {
    chart.options.title.fontSize = titleFontSize;
    chart.options.legend.labels.fontSize = legendFontSize;
    chart.options.tooltips.titleFontSize = tooltipsFontSize;
    chart.options.tooltips.bodyFontSize = tooltipsFontSize;
    chart.options.scales.yAxes[0].scaleLabel.fontSize = labelsFontSize;
    chart.options.scales.xAxes[0].scaleLabel.fontSize = labelsFontSize;
    chart.options.scales.yAxes[0].ticks.fontSize = ticksFontSize;
    chart.options.scales.xAxes[0].ticks.fontSize = ticksFontSize;
  }
}

function onResizeChart(chart, chartSize) {
  updateChart(chart, chartSize);
}

function createChart(chartName, title, labels, data, minY, maxY, xAxisLabel, yAxisLabel,
  typeOfChart, yAxisTicksEnding, tooltipTitlePrefix, tooltipLabelEnding) {
  const ctx = $('#' + chartName);

  let chart = new Chart(ctx, {
    type: typeOfChart,
    data: {
      labels: labels,
      datasets: generateStartDatasetArray(data),
    },
    options: chartDefaultConfig,
  });

  if (maxY != 0) {
    chart.options.scales.yAxes[0].ticks.suggestedMin = minY;
    chart.options.scales.yAxes[0].ticks.suggestedMax = maxY;
  }

  if (title == roundPointsTitle) {
    chart.options.tooltips.callbacks.label = (tooltipItem, data) => {
      const teamName = data.datasets[tooltipItem.datasetIndex].label;
      let label = teamName || '';

      if (label) {
        label += ': ';
      }
      label += tooltipItem.yLabel;
      label += tooltipLabelEnding;

      if (teamStandings[teamName].opposingTeams[tooltipItem.index] !== '') {
        label += ' | ' + teamStandings[teamName].opposingTeams[tooltipItem.index]
          + ' ' + teamStandings[teamName].matchResults[tooltipItem.index]
          + ' (' + teamStandings[teamName].matchDates[tooltipItem.index] + ') ';
      }

      return label;
    }
  }
  else {
    chart.options.tooltips.callbacks.label = (tooltipItem, data) => {
      let label = data.datasets[tooltipItem.datasetIndex].label || '';

      if (label) {
        label += ': ';
      }
      label += tooltipItem.yLabel;
      label += tooltipLabelEnding;
      return label;
    };
  }

  chart.options.title.text = title;
  chart.options.tooltips.callbacks.title = (tooltipItems, data) => {
    const title = tooltipTitlePrefix + ': ' + tooltipItems[0].xLabel;
    return title;
  };
  chart.options.scales.yAxes[0].scaleLabel.labelString = yAxisLabel;
  chart.options.scales.xAxes[0].scaleLabel.labelString = xAxisLabel;
  chart.options.scales.yAxes[0].ticks.callback = (value, index, values) => {
    return value + yAxisTicksEnding;
  };
  chart.options.onResize = onResizeChart;

  const chartDisplaySize = getChartDisplaySize(chartName);
  updateChart(chart, chartDisplaySize);
  return chart;
}

$(document).ready(() => {
  goalsInIntervalsChart = createChart(
    'goalsInIntervalsChartArea',
    goalsInIntervalsTitle,
    timeIntervalsTexts,
    goalsInIntervalsPercent, 0, 0,
    goalsInIntervalsXLabel,
    goalsInIntervalsYLabel,
    'bar', '%',
    goalsInIntervalsTooltipTitle, '%');

  matchGoalsChart = createChart(
    'matchGoalsChartArea',
    matchGoalsTitle,
    matchGoalsLabels, matchGoalsPct, 0, 0,
    matchGoalsXLabel,
    matchGoalsYLabel, 'bar', '%',
    matchGoalsTooltipTitle, '%');

  roundPointsChart = createChart(
    'roundPointsChartArea',
    roundPointsTitle,
    roundPointsLabels, null, 0, 20,
    roundPointsXLabel,
    roundPointsYLabel, 'line', 'pts',
    roundPointsTooltipTitle, 'pts');
});

function updateGoalsInIntervals(chart, teamName, index) {
  const statsRequestData = getStatsRequestData(teamName);

  $.post(goalsInIntervalsURI, statsRequestData, null, 'json')
    .done((data) => {
      updateChartData(chart, data, index);
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

function updateChartData(chart, data, index) {
  chart.data.datasets[index].data = data;
  chart.update();
}

$('#teamsList > option').mousedown(function() {
  const teamName = $(this).text(),
    id = $(this).val();

  firstTeam = {
    name: teamName,
    id: parseInt(id),
  }
});

/** Create array of selected teams in order: from team selected in "mousedown" event to currentTeamId*/
function createSelectedTeamsArray(currentTeamId) {
  let selectedTeams = $('#teamsList > option').filter(function(index) {
    const id = index + 1;
    return id >= firstTeam.id && id <= currentTeamId;
  }).get();

  if (firstTeam.id > currentTeamId) {
    selectedTeams = $('#teamsList > option').filter(function(index) {
      const id = index + 1;
      return id >= currentTeamId && id <= firstTeam.id
    }).get();

    selectedTeams = selectedTeams.reverse();
  }
  return selectedTeams;
}

function removeTeamFromCharts(teamName) {
  removeChartDataset(goalsInIntervalsChart, teamName);
  removeChartDataset(matchGoalsChart, teamName);
  removeChartDataset(roundPointsChart, teamName);
}

function addTeamToCharts(teamName, id) {
  addChartDataset(goalsInIntervalsChart, goalsInIntervalsURI, teamName, id);
  addChartDataset(matchGoalsChart, matchGoalsURI, teamName, id);
  addChartDataset(roundPointsChart, roundPointsURI, teamName, id);
}

// TODO: Change to async function - waiting for dataset added to the roundPointsChart
function changeTeamPointStyle(chart, teamName, pointStyle) {
  const index = chart.data.datasets.findIndex(
    dataset => dataset.label === teamName);

  // In case of new team is still not added to chart - its point style will change after receiving data
  // OR when the team has already been removed from the chart (rather impossible)
  if (index != -1) {
    chart.data.datasets[index].pointStyle = pointStyle;
  }
  chart.update();
}

$('#teamsList > option').mouseup(function() {
  const currentTeamId = parseInt($(this).val());
  const selectedTeams = createSelectedTeamsArray(currentTeamId);
  let teamsInNewOrder = Object.assign({}, teamsInSelectionOrder);

  $('#teamsList > option').each(function(index, team) {
    const teamName = $(team).text();

    if (!($(team).prop('selected'))) {
      if (teamsInSelectionOrder[teamName] == teamName) {
        delete teamsInNewOrder[teamName];
        delete teamsInSelectionOrder[teamName];
        removeTeamFromCharts(teamName);

        if (teamName === chartFirstTeamName) {
          isFirstTeamRemoved = true;
        }
      }
    }
    else if (selectedTeams.findIndex(teamItem => $(teamItem).text() === teamName) !== -1) {
      // Order from newly selected teams is more important than the existing order
      delete teamsInNewOrder[teamName];
      if (teamName === firstTeam.name && teamName !== chartFirstTeamName) {
        isNewFirstTeamSelected = true;
      }
    }
  });

  $(selectedTeams).each(function(index, teamItem) {
    const teamName = $(teamItem).text();
    const id = parseInt($(teamItem).val());

    if ($(teamItem).prop('selected')) {
      teamsInNewOrder[teamName] = teamName;
      // If there isn't the selected team on the chart yet
      if (teamsInSelectionOrder[teamName] !== teamName) {
        addTeamToCharts(teamName, id);
      }
    }
  });

  if (!isFirstTeamRemoved && isNewFirstTeamSelected) {
    // Update point style for old first team on the chart
    changeTeamPointStyle(roundPointsChart, chartFirstTeamName, 'circle');
  }
  teamsInSelectionOrder = Object.assign({}, teamsInNewOrder);

  if (isNewFirstTeamSelected || isFirstTeamRemoved) {
    for (const teamName in teamsInSelectionOrder) {
      chartFirstTeamName = teamName;
      break;
    }
    let firstTeamOppCrests = teamStandings[chartFirstTeamName].opponentCrests;
    // Update point style for new first team on the chart
    changeTeamPointStyle(roundPointsChart, chartFirstTeamName, firstTeamOppCrests);
  }
  isFirstTeamRemoved = false;
  isNewFirstTeamSelected = false;
});

$('#changeRounds').click(() => {
  updateSelectedRounds();

  goalsInIntervalsChart.data.datasets.forEach((dataset, i) => {
    let teamName = '*';

    if (dataset.label !== leagueName) {
      teamName = dataset.label;
    }
    updateGoalsInIntervals(goalsInIntervalsChart, teamName, i);
    updateMatchGoals(matchGoalsChart, teamName, i);
  });
});

function extractRoundPointsData(teamName, data) {
  let labels = Object.keys(data).map(label => parseInt(label));
  const values = Object.values(data),
    maxRound = labels[labels.length - 1];

  labels = Array.from(Array(maxRound).keys(), x => x + 1);

  values.forEach((value, i) => {
    if (value != null && value != undefined) {
      let opponentCrest = new Image();
      opponentCrest.src = value.Opponent + '.png';
      teamStandings[teamName].points[i] = value.Points;
      teamStandings[teamName].tablePositions[i] = value.TablePosition;
      teamStandings[teamName].opponentCrests[i] = opponentCrest;
      teamStandings[teamName].opposingTeams[i] = value.OpposingTeams;
      teamStandings[teamName].matchResults[i] = value.MatchResult;
      teamStandings[teamName].matchDates[i] = value.MatchDate;
    }
    else {
      let blankCrest = new Image();
      blankCrest.src = 'blank.png';

      if (i == 0) {
        teamStandings[teamName].points[i] = 0;
      }
      else {
        teamStandings[teamName].points[i] = teamStandings[teamName].points[i - 1];
      }
      teamStandings[teamName].opponentCrests[i] = blankCrest;
      teamStandings[teamName].opposingTeams[i] = '';
      teamStandings[teamName].matchResults[i] = '';
      teamStandings[teamName].matchDates[i] = '';
    }
  });

  return new ChartData(teamStandings[teamName].points, labels);
}

function extractMatchGoalsData(data) {
  let labels = Array.from(Array(data.length).keys());
  return new ChartData(data, labels);
}

function updateMatchGoals(chart, teamName, index) {
  const statsRequestData = getStatsRequestData(teamName);

  $.post(matchGoalsURI, statsRequestData, null, 'json')
    .done((data) => {
      const chartData = extractMatchGoalsData(data);
      // Update chart labels when there is a request for whole league
      if (index === 0) {
        chart.data.labels = chartData.labels;
      }
      updateChartData(chart, chartData.data, index)
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

class ChartData {
  constructor(data, labels) {
    this.data = data;
    this.labels = labels;
  }
}