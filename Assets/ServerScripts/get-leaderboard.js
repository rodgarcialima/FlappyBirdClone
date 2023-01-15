const { DataApi } = require("@unity-services/cloud-save-1.2");

const GLOBAL_PLAYER_ID = "yyZKcUEy3OhPkqMatYRM4yFL38wQ";

module.exports = async ({ params, context, logger }) => {
  const { projectId, playerId } = context;

  const cloudSaveAPI = new DataApi(context); 

  const { data } = await cloudSaveAPI.getItems(projectId, GLOBAL_PLAYER_ID, ['leaderboard']);
  const leaderboard = data.results[0].value;

  return {
    players: leaderboard.map(l => l.name),
    scores: leaderboard.map(l => l.score)
  }
};

