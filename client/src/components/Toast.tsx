interface ToastProps {
  message: string | null;
  tone?: "success" | "error";
}

export const Toast = ({ message, tone = "success" }: ToastProps) => {
  if (!message) return null;
  return <div className={`toast toast-${tone}`}>{message}</div>;
};
