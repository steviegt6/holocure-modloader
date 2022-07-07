import kleur from "kleur";

// stolen from kleur with small modifications
function init(x: number, y: number, z = "38;5;") {
  let rgx = new RegExp(`\\x1b\\[${y}m`, "g");
  let open = `\x1b[${z}${x}m`,
    close = `\x1b[${y}m`;

  return function (txt: string | null) {
    if (/*!$.enabled ||*/ txt == null) return txt;
    return (
      open +
      (!!~("" + txt).indexOf(close) ? txt.replace(rgx, close + open) : txt) +
      close
    );
  };
}

const pinkish = init(219, 39);
const brightWhite = init(15, 39);

export { kleur as color, pinkish, brightWhite };
