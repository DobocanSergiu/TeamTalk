import styles from "./RoomDrive.module.css";
import wordIcon from "../../../Assets/word.png";
import excelIcon from "../../../Assets/excel.png";
import imageIcon from "../../../Assets/image.png";
import pdfIcon from "../../../Assets/pdf.png";
import powerPointIcon from "../../../Assets/powerpoint.png";
import textIcon from "../../../Assets/text.png";
import videoIcon from "../../../Assets/video.png";
import unknownIcon from "../../../Assets/unknown.png";
function RoomDrive({ roomFiles, toggleRoomListModal }) {
  const handleDownload = (fileName, fileData) => {
    const link = document.createElement("a");
    link.href = fileData;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };
  return (
    <div className={styles.parent}>
      <div className={styles.header} onClick={toggleRoomListModal}>
        Room Files
      </div>
      <hr className={styles.separator} />

      <ul className={styles.listContainer}>
        {roomFiles.map((file) => {
          let icon;
          switch (file.type) {
            case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
              icon = wordIcon;
              break;
            case "application/msword":
              icon = wordIcon;
              break;
            case "application/pdf":
              icon = pdfIcon;
              break;
            case "image/png":
              icon = imageIcon;
              break;
            case "image/jpeg":
              icon = imageIcon;
              break;
            case "application/vnd.ms-excel":
              icon = excelIcon;
              break;
            case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
              icon = excelIcon;
              break;
            case "application/vnd.ms-powerpoint":
              icon = powerPointIcon;
              break;
            case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
              icon = powerPointIcon;
              break;
            case "text/plain":
              icon = textIcon;
              break;
            case "video/mp4":
              icon = videoIcon;
              break;
            case "video/webm":
              icon = videoIcon;
              break;
            case "video/mpeg":
              icon = videoIcon;
              break;
            default:
              icon = unknownIcon;
          }

          return (
            <li
              className={styles.listItem}
              onClick={() => handleDownload(file.name, file.data)}
            >
              <img src={icon} alt={`${file.type} icon`} />
              <div>{file.name}</div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}

export default RoomDrive;
