import styles from "./FileMessageComponent.module.css";
import wordIcon from "../../../Assets/word.png";
import pdfIcon from "../../../Assets/pdf.png";
import imageIcon from "../../../Assets/image.png";
import excelIcon from "../../../Assets/excel.png";
import powerPointIcon from "../../../Assets/powerpoint.png";
import textIcon from "../../../Assets/text.png";
import videoIcon from "../../../Assets/video.png";
import unknownIcon from "../../../Assets/unknown.png";

function FileMessageComponent({ Username, FileName, FileType, FileData }) {
  // Function to get the appropriate icon based on file type
  const getFileIcon = (fileType) => {
    let icon;
    switch (fileType) {
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
        break;
    }
    return icon;
  };

  // Function to handle file download
  const handleDownload = () => {
    const link = document.createElement("a");
    link.href = FileData;
    link.download = FileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <li className={styles.parent}>
      <div className={styles.username}>{Username}</div>
      <div
        className={styles.fileContainer}
        onClick={handleDownload}
        role="button"
        tabIndex={0}
      >
        <div className={styles.fileIcon}>
          <img
            src={getFileIcon(FileType)}
            alt={`${FileType} icon`}
            width={24}
            height={24}
          />
        </div>
        <div className={styles.fileInfo}>
          <div className={styles.fileName}>{FileName}</div>
          <div className={styles.fileDetails}>{FileType}</div>
        </div>
      </div>
    </li>
  );
}

export default FileMessageComponent;
